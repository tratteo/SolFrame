using Newtonsoft.Json;
using Solnet.Rpc.Messages;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Solnet.Rpc.Core.Http
{
    public abstract class JsonRpcClient
    {
        private readonly string url;

        protected JsonRpcClient(string url)
        {
            this.url = url;
        }

        protected async Task<RequestResult<T>> SendRequest<T>(JsonRpcRequest req)
        {
            var requestJson = JsonConvert.SerializeObject(req);

            //UnityEngine.Debug.Log($"\tRequest: {requestJson}");
            HttpResponseMessage response = null;
            RequestResult<T> result = null;

            using (var request = new UnityWebRequest(url, "POST"))
            {
                var jsonToSend = new System.Text.UTF8Encoding().GetBytes(requestJson);
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                //Send the request then wait here until it returns
                request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("Error While Sending: " + request.error);
                    //result = new RequestResult<T>(400, request.error.ToString());
                }
                else
                {
                    while (!request.isDone)
                    {
                        //UnityEngine.Debug.Log("Delay");
                        await Task.Yield();
                    }

                    var res = JsonConvert.DeserializeObject<JsonRpcResponse<T>>(request.downloadHandler.text);
                    result = new RequestResult<T>(response);
                    if (res.Result != null)
                    {
                        result.WasRequestSuccessfullyHandled = true;
                        result.Result = res.Result;
                    }
                    else
                    {
                        var errorRes = JsonUtility.FromJson<JsonRpcErrorResponse>("");
                        if (errorRes != null && errorRes.Error != null)
                        {
                            result.WasRequestSuccessfullyHandled = false;
                            result.Reason = errorRes.Error.Message;
                            result.ServerErrorCode = errorRes.Error.Code;
                        }
                        else
                        {
                            result.Reason = "Something wrong happened.";
                        }
                    }
                }
            }

            return result;
        }
    }
}