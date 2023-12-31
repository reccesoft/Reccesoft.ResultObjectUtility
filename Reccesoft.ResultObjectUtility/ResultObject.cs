﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TS
{
    public class ResultObject
    {
        public const string DefaultOkMessage = "Operation Succeeded";
        public const string DefaultErrorMessage = "Operation did not complete successfully";
        public const string DefaultNoActionTakenMessage = "No Action taken";
        public const string DefaultGenericException = "An exception occurred";
        public const string DefaultErrorMessageDatabaseTransaction = "A database transaction error occurred";

        public bool IsSuccess { get; set; }
        public bool HasRealExceptionMessage { get; set; }
        public List<string> Messages { get; set; } = new List<string>();


        public ResultObject()
        {
        }

        public ResultObject(ResultObject? resultObject)
        {
            resultObject ??= NoActionTakenResultObject();
            this.IsSuccess = resultObject.IsSuccess;
            this.HasRealExceptionMessage = resultObject.HasRealExceptionMessage;
            this.Messages = new List<string>(resultObject.Messages.ToList());
        }

        public static ResultObject NoActionTakenResultObject() => Error(DefaultNoActionTakenMessage);

        public ResultObject(Exception exception, bool useRealExceptionMessage = false)
        {
            IsSuccess = false;
            Messages = new List<string>() { useRealExceptionMessage ? exception.Message : DefaultGenericException };
            HasRealExceptionMessage = useRealExceptionMessage;
        }

        public static ResultObject Ok(string? okMessage = null)
        {
            ResultObject vtr = new ResultObject();
            vtr.SetOk(okMessage);
            return vtr;
        }

        public static ResultObject Error(string? errorMessage = null)
        {
            ResultObject vtr = new ResultObject();
            vtr.SetError(errorMessage);
            return vtr;
        }

        public static ResultObject NotFound(string? itemName = null)
        {
            ResultObject vtr = new ResultObject();
            vtr.SetNotFound(itemName);
            return vtr;
        }

        public static ResultObject OkOrNotFound<T>(T item, string? itemNameNotFound = null, string? okMessageIfItemIsFound = null)
        {
            ResultObject vtr = new ResultObject();
            vtr.SetOkOrNotFoundOnlyDontAttatchObject(item, itemNameNotFound, okMessageIfItemIsFound);
            return vtr;
        }

        public static ResultObject OkOrDatabaseError(int numberOfRowsChanged, string? databaseErrorMessage = null, string? databaseSuccessMessage = null)
        {
            ResultObject vtr = new ResultObject();
            vtr.SetOkOrDatabaseError(numberOfRowsChanged, databaseErrorMessage, databaseSuccessMessage);
            return vtr;
        }

        public void SetOk(IEnumerable<string>? messages = null)
        {
            IsSuccess = true;
            Messages = messages?.Any() ?? false ? messages.ToList() : new List<string>() { DefaultOkMessage };
        }

        public void SetOk(string? message)
        {
            message = String.IsNullOrWhiteSpace(message) ? DefaultOkMessage : message;
            SetOk(new List<string>() { message });
        }

        public void SetError(IEnumerable<string>? messages = null)
        {
            IsSuccess = false;
            Messages = messages?.Any() ?? false ? messages.ToList() : new List<string>() { DefaultErrorMessage };
        }

        public void SetError(string? message)
        {
            message = String.IsNullOrWhiteSpace(message) ? DefaultErrorMessage : message;
            SetError(new List<string>() { message });
        }

        public void SetError(Exception ex, bool setRealExceptionMessage = false)
        {
            SetError(setRealExceptionMessage ? ex.Message : DefaultGenericException);
            HasRealExceptionMessage = setRealExceptionMessage;
        }

        public void SetNotFound(string? itemName = null)
        {
            SetNotFoundBase(itemName);
        }

        protected void SetNotFoundBase(string? itemName)
        {
            itemName = String.IsNullOrWhiteSpace(itemName) ? "Object Requested" : itemName;
            string message = $"{itemName} Not Found";
            IsSuccess = false;
            Messages = new List<string>() { message };
        }

        public void SetOkOrNotFoundOnlyDontAttatchObject<T>(T item, string? itemNameNotFound = null, string? okMessageIfItemIsFound = null)
        {
            if (item == null)
            {
                SetNotFound(itemNameNotFound);
            }
            else
            {
                SetOk(okMessageIfItemIsFound);
            }
        }

        public void SetOkOrDatabaseError(int numberOfRowsChanged, string? databaseErrorMessage = null, string? databaseSuccessMessage = null)
        {
            if (string.IsNullOrWhiteSpace(databaseErrorMessage))
            {
                databaseErrorMessage = DefaultErrorMessageDatabaseTransaction;
            }

            if (numberOfRowsChanged < 1)
            {
                SetError(databaseErrorMessage);
            }
            else
            {
                SetOk(databaseSuccessMessage);
            }
        }

        public static implicit operator bool(ResultObject? resultObject) => resultObject?.IsSuccess ?? false;
        public static implicit operator ResultObject(bool isSuccess) => isSuccess ? Ok() : Error();
        public static implicit operator ResultObject(Exception ex) => new ResultObject(ex, true);
        public static implicit operator List<string>(ResultObject? resultObject) 
        {
            resultObject ??= NoActionTakenResultObject();
            return resultObject.Messages ?? new List<string>();
        } 
    }

    public class ResultObject<T> : ResultObject where T : class
    {
        /// <summary>
        /// It can be null depending on the type
        /// </summary>
        public T? ReturnedObject { get; set; }
        public ResultObject() : base()
        {
        }

        public ResultObject(ResultObject? resultObject)
        {
            resultObject ??= NoActionTakenResultObject();
            IsSuccess = resultObject.IsSuccess;
            HasRealExceptionMessage = resultObject.HasRealExceptionMessage;
            Messages = new List<string>(resultObject.Messages.ToList());
        }
        new public static ResultObject<T> NoActionTakenResultObject() => Error(DefaultNoActionTakenMessage);
        public void SetOkAndAttachReturnedObject(T itemToReturn, IEnumerable<string>? messages = null)
        {
            SetOk(messages);
            ReturnedObject = itemToReturn;
        }

        new public void SetNotFound(string? itemName = null)
        {
            SetNotFoundBase(itemName);
        }

        public void SetOkAndAttachReturnedObject(T itemToReturn, string? message)
        {
            SetOk(message);
            ReturnedObject = itemToReturn;
        }

        public void SetOkOrNotFoundAndAttachReturnedObject(T? item, string? itemNameNotFound = null, string? okMessageIfItemIsFound = null)
        {
            ReturnedObject = item;
            SetOkOrNotFoundOnlyDontAttatchObject(item, itemNameNotFound, okMessageIfItemIsFound);
        }

        public async Task SetOkOrNotFoundAndAttachReturnedObjectFromMethod<Y>(Y? item, Func<Task<T>> getObjectToReturnIfObjectWasFound, string? itemNameNotFound = null, string? okMessageIfItemIsFound = null) where Y : class
        {
            SetOkOrNotFoundOnlyDontAttatchObject(item, itemNameNotFound, okMessageIfItemIsFound);
            if (IsSuccess && getObjectToReturnIfObjectWasFound != null)
            {
                ReturnedObject = await getObjectToReturnIfObjectWasFound();
            }
        }

        public async Task SetOkOrDatabaseErrorAndAttachReturnedObject(int numberOfRowsChanged, Func<Task<T?>> getObjectToReturnOnlyWhenDatabaseTransactionSuccessful, string? databaseErrorMessage = null, string? databaseSuccessMessage = null)
        {
            SetOkOrDatabaseError(numberOfRowsChanged, databaseErrorMessage, databaseSuccessMessage);
            if (IsSuccess && getObjectToReturnOnlyWhenDatabaseTransactionSuccessful != null)
            {
                ReturnedObject = await getObjectToReturnOnlyWhenDatabaseTransactionSuccessful();
            }
        }

        public Task SetOkOrDatabaseErrorAndAttachReturnedObject(int numberOfRowsChanged, T? objectToReturnOnlyWhenDatabaseTransactionSuccessful, string? databaseErrorMessage = null, string? databaseSuccessMessage = null) => SetOkOrDatabaseErrorAndAttachReturnedObject(numberOfRowsChanged, () => Task.FromResult(objectToReturnOnlyWhenDatabaseTransactionSuccessful), databaseErrorMessage, databaseSuccessMessage);

        public static ResultObject<T> OkAndAttachReturnedObject(T objectToReturn, string? okMessage = null)
        {
            ResultObject<T> vtr = new ResultObject<T>();
            vtr.SetOkAndAttachReturnedObject(objectToReturn, okMessage);
            return vtr;
        }


        public static ResultObject<T> OkOrNotFoundAndAttachReturnedObject(T? item, string? itemNameNotFound = null, string? okMessageIfItemIsFound = null)
        {
            ResultObject<T> vtr = new ResultObject<T>();
            vtr.SetOkOrNotFoundAndAttachReturnedObject(item, itemNameNotFound, okMessageIfItemIsFound);
            return vtr;
        }

        public static async Task<ResultObject<T>> OkOrNotFoundAndAttachReturnedObjectFromMethod<Y>(Y? item, Func<Task<T>> getObjectToReturnIfObjectWasFound, string? itemNameNotFound = null, string? okMessageIfItemIsFound = null) where Y : class
        {
            ResultObject<T> vtr = new ResultObject<T>();
            await vtr.SetOkOrNotFoundAndAttachReturnedObjectFromMethod(item, getObjectToReturnIfObjectWasFound, itemNameNotFound, okMessageIfItemIsFound);
            return vtr;
        }

        public static async Task<ResultObject<T>> OkOrDatabaseErrorAndAttachReturnedObject(int numberOfRowsChanged, Func<Task<T?>> getObjectToReturnOnlyWhenDatabaseTransactionSuccessful, string? databaseErrorMessage = null, string? databaseSuccessMessage = null)
        {
            ResultObject<T> vtr = new ResultObject<T>();
            await vtr.SetOkOrDatabaseErrorAndAttachReturnedObject(numberOfRowsChanged, getObjectToReturnOnlyWhenDatabaseTransactionSuccessful, databaseErrorMessage, databaseSuccessMessage);
            return vtr;
        }

        public static ResultObject<T> Error(IEnumerable<string>? errorMessages = null)
        {
            ResultObject<T> vtr = new ResultObject<T>();
            vtr.SetError(errorMessages);
            return vtr;
        }

        new public static ResultObject<T> Error(string errorMessage)
        {
            ResultObject<T> vtr = new ResultObject<T>();
            vtr.SetError(errorMessage);
            return vtr;
        }

        public void CopyOtherResultObject(ResultObject? resultObject, T objectToAttachCanBeNull)
        {
            resultObject ??= ResultObject.NoActionTakenResultObject();
            IsSuccess = resultObject.IsSuccess;
            Messages = resultObject.Messages.ToList();
            HasRealExceptionMessage = resultObject.HasRealExceptionMessage;
            ReturnedObject = objectToAttachCanBeNull;
        }

        public static implicit operator T?(ResultObject<T>? resultObject) => resultObject?.ReturnedObject;
        public static implicit operator ResultObject<T>(T? objectToReturnOrNotFoundError)
        {
            ResultObject<T> vtr = new ResultObject<T>();
            vtr.SetOkOrNotFoundAndAttachReturnedObject(objectToReturnOrNotFoundError);
            return vtr;
        }
        public static implicit operator bool(ResultObject<T>? resultObject) => resultObject?.IsSuccess ?? false;
        public static implicit operator ResultObject<T>(Exception ex)
        {
            ResultObject<T> vtr = new ResultObject<T>();
            vtr.SetError(ex, true);
            return vtr;
        }
        public static implicit operator List<string>(ResultObject<T>? resultObject)
        {
            resultObject ??= NoActionTakenResultObject();
            return resultObject.Messages ?? new List<string>();
        }

    }
}
