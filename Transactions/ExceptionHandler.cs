using Microsoft.AspNetCore.Mvc;

namespace Transactions;

public class ExceptionHandler<TReturnValue>
{
    private TReturnValue? _result;
    private static bool _success = false;
    private static ObjectResult? _handledException = null;
    private (Type exceptionType, int statusCode)[] _exceptions = [];

    public ExceptionHandler() { }
    public ExceptionHandler(Func<TReturnValue> initialMethod, params (Type exceptionType, int statusCode)[] exceptions)
    {
        _exceptions = exceptions;

        try
        {              
            _result = initialMethod();
            _success = true;
        }
        catch (Exception ex)
        {            
            _handledException = HandleException(ex);
        }
    }    

    public async Task<ActionResult> ExecuteAsync(Func<Task<TReturnValue>> initialMethodAsync, params (Type exceptionType, int statusCode)[] exceptions)
    {
        _exceptions = exceptions;

        try
        {
            _result = await initialMethodAsync();
            _success = true;

            return new ObjectResult(_result);
        }
        catch (Exception ex)
        {
            _handledException = HandleException(ex);
            return _handledException ?? new ObjectResult(StatusCodes.Status500InternalServerError);
        }
    }

    public ObjectResult HandleException(Exception ex)
    {
        var originalExceptionType = ex.GetType();
        var errorPayload = new 
        { 
            message = ex.Message, 
            errorType = originalExceptionType.Name 
        };

        var (exceptionType, statusCode) = _exceptions.FirstOrDefault(e => e.exceptionType.Name.Equals(originalExceptionType.Name));
        
        return new ObjectResult(errorPayload) { StatusCode = exceptionType is not null ? statusCode : StatusCodes.Status500InternalServerError};
    }

    public static implicit operator ActionResult(ExceptionHandler<TReturnValue> transaction)
    {    
        if(_success)
        {
            return new ObjectResult(transaction._result);
        }
        else
        {
            return _handledException ?? new ObjectResult(StatusCodes.Status500InternalServerError);
        }
    }    
}