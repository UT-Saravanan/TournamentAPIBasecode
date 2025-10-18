using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Threading;
using UnityEngine.Networking;
using System.Text;
using System;
[System.Serializable]
public class BetHistoryController
{

    [SerializeField]
    public List<BetHistoryModel> betHistories = new List<BetHistoryModel>();
    [SerializeField]
    public APIFormant formant;

    public void GetTournamentBetList( string url,string user_id, Action<bool> action)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();

        parameters.Add("request_type", "betHistory");
        parameters.Add("user_id", user_id);

        string body = JsonConvert.SerializeObject(parameters);
        WebApiManager.Instance.GetNetWorkCallPostMethodUsingJson(url, body, (success, error, body) => {
            if (success)
            {
                formant = JsonUtility.FromJson<APIFormant>(body);
                formant.ParseBody();

                betHistories = JsonConvert.DeserializeObject<List<BetHistoryModel>>(formant.response.parse.data);
                foreach (var bet in betHistories)
                    bet.ParseBody();
            }
            else
            {
                Debug.Log("failed to load tournament");
                
            }
            action.Invoke(success);

        }
        );
    }
}
