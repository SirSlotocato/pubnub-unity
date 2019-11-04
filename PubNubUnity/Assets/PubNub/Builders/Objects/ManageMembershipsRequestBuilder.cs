using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace PubNubAPI
{
    public class PNMembershipsInput{
        string ID;
        Dictionary<string, object> Custom;
    }
    public class PNMembershipsRemove{
        string ID;
    }
    public class ManageMembershipsRequestBuilder: PubNubNonSubBuilder<ManageMembershipsRequestBuilder, PNMembershipsResult>, IPubNubNonSubscribeBuilder<ManageMembershipsRequestBuilder, PNMembershipsResult>
    {        
        private string ManageMembershipsUserID { get; set;}
        private int ManageMembershipsLimit { get; set;}
        private string ManageMembershipsEnd { get; set;}
        private string ManageMembershipsStart { get; set;}
        private bool ManageMembershipsCount { get; set;}
        private PNMembershipsInclude[] ManagerMembershipsInclude { get; set;}
        private List<PNMembershipsInput> ManageMembershipsAdd { get; set;}
        private List<PNMembershipsInput> ManageMembershipsUpdate { get; set;}
        private List<PNMembershipsRemove> ManageMembershipsRemove { get; set;}
        
        public ManageMembershipsRequestBuilder(PubNubUnity pn): base(pn, PNOperationType.PNManageMembershipsOperation){
        }

        #region IPubNubBuilder implementation
        public void Async(Action<PNMembershipsResult, PNStatus> callback)
        {
            this.Callback = callback;
            base.Async(this);
        }
        #endregion

        public ManageMembershipsRequestBuilder UserID(string id){
            ManageMembershipsUserID = id;
            return this;
        }

        public ManageMembershipsRequestBuilder Include(PNMembershipsInclude[] include){
            ManagerMembershipsInclude = include;
            return this;
        }
        public ManageMembershipsRequestBuilder Limit(int limit){
            ManageMembershipsLimit = limit;
            return this;
        }

        public ManageMembershipsRequestBuilder Start(string start){
            ManageMembershipsStart = start;
            return this;
        }
        public ManageMembershipsRequestBuilder End(string end){
            ManageMembershipsEnd = end;
            return this;
        }
        public ManageMembershipsRequestBuilder Count(bool count){
            ManageMembershipsCount = count;
            return this;
        }
        public ManageMembershipsRequestBuilder Add(List<PNMembershipsInput> add){
            ManageMembershipsAdd = add;
            return this;
        }
        public ManageMembershipsRequestBuilder Update(List<PNMembershipsInput> update){
            ManageMembershipsUpdate = update;
            return this;
        }
        public ManageMembershipsRequestBuilder Remove(List<PNMembershipsRemove> remove){
            ManageMembershipsRemove = remove;
            return this;
        }

        protected override void RunWebRequest(QueueManager qm){
            RequestState requestState = new RequestState ();
            requestState.OperationType = OperationType;
            requestState.httpMethod = HTTPMethod.Post;

            var cub = new { 
                add = ManageMembershipsAdd.ToArray(), 
                update = ManageMembershipsUpdate.ToArray(),
                remove = ManageMembershipsRemove.ToArray(),
            };

            string jsonUserBody = Helpers.JsonEncodePublishMsg (cub, "", this.PubNubInstance.JsonLibrary, this.PubNubInstance.PNLog);
            #if (ENABLE_PUBNUB_LOGGING)
            this.PubNubInstance.PNLog.WriteToLog (string.Format ("jsonUserBody: {0}", jsonUserBody), PNLoggingMethod.LevelInfo);
            #endif
            requestState.POSTData = jsonUserBody;

            string[] includeString = (ManagerMembershipsInclude==null) ? new string[]{} : ManagerMembershipsInclude.Select(a=>a.GetDescription().ToString()).ToArray();

            Uri request = BuildRequests.BuildObjectsManageMembershipsRequest(
                    ManageMembershipsUserID,
                    ManageMembershipsLimit,
                    ManageMembershipsStart,
                    ManageMembershipsEnd,
                    ManageMembershipsCount,
                    string.Join(",", includeString),
                    this.PubNubInstance,
                    this.QueryParams
                );
            base.RunWebRequest(qm, request, requestState, this.PubNubInstance.PNConfig.NonSubscribeTimeout, 0, this); 
        }

        protected override void CreatePubNubResponse(object deSerializedResult, RequestState requestState){
            object[] c = deSerializedResult as object[];
            
            PNMembershipsResult pnManageMembershipsResult = new PNMembershipsResult();
            pnManageMembershipsResult.Data = new List<PNMemberships>();
            PNStatus pnStatus = new PNStatus();

            try{
                Dictionary<string, object> dictionary = deSerializedResult as Dictionary<string, object>;
                
                if(dictionary != null) {
                    object objData;
                    dictionary.TryGetValue("data", out objData);
                    if(objData!=null){
                        object[] objArr = objData as object[];
                        foreach (object data in objArr){
                            Dictionary<string, object> objDataDict = data as Dictionary<string, object>;
                            if(objDataDict!=null){
                                PNMemberships pnMemberships = ObjectsHelpers.ExtractMemberships(objDataDict);
                                pnManageMembershipsResult.Data.Add(pnMemberships);
                            }  else {
                                pnManageMembershipsResult = null;
                                pnStatus = base.CreateErrorResponseFromException(new PubNubException("objDataDict null"), requestState, PNStatusCategory.PNUnknownCategory);
                            }  
                        }
                    }  else {
                        pnManageMembershipsResult = null;
                        pnStatus = base.CreateErrorResponseFromException(new PubNubException("objData null"), requestState, PNStatusCategory.PNUnknownCategory);
                    }  
                }
            } catch (Exception ex){
                pnManageMembershipsResult = null;
                pnStatus = base.CreateErrorResponseFromException(ex, requestState, PNStatusCategory.PNUnknownCategory);
            }
            Callback(pnManageMembershipsResult, pnStatus);

        }

    }
}
