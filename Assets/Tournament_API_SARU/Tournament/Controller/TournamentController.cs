using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class TournamentController
{
    public string APIURL = "https://d86vo3ohx1r0e.cloudfront.net/";
    [SerializeField]
    public APIFormant formant;
    public string userID;
    [SerializeField]
    public List<TournamentModel> TournamentList = new List<TournamentModel>();
    public BetHistoryController betHistory;


    public string output;



    public void BuyTicketsForTournament(string tournament_id, string round_id, string session_token, TicketList ticket_data, int amount, string userToken, string currency, string meta_data, Action<bool,string> action)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();

        parameters.Add("request_type", "add_tickets");
        parameters.Add("tournament_id", tournament_id);
        parameters.Add("round_id", round_id);
        parameters.Add("user_id", userID);
        parameters.Add("session_token", session_token);
        parameters.Add("ticket_data", JsonUtility.ToJson(ticket_data));
        parameters.Add("bet_amount", amount.ToString());
        parameters.Add("user_token", userToken);
        parameters.Add("is_bot", "0");
        parameters.Add("currency", currency);
        parameters.Add("meta_data", meta_data);
        parameters.Add("platform", JsonUtility.ToJson(new DeviceInfo()));

        string body = JsonConvert.SerializeObject(parameters);
        WebApiManager.Instance.GetNetWorkCallPostMethodUsingJson(APIURL, body, (success, error, body) =>
        {
            if (success)
            {

                FetchTournament(APIURL, userID, tournament_id, (success, updatedtournament) => {
                    int index = TournamentList.FindIndex(x => x.tournament_id == updatedtournament.tournament_id);

                    TournamentList[index].round_data = updatedtournament.round_data;
                    if (TournamentList[index].round_data[1].status == TournamentStatus.End)
                    {
                        TournamentList[index].status = TournamentStatus.End;
                    }
                    TournamentList[index].action?.Invoke(TournamentList[index]);

                    Debug.Log("invoke tournamnet changes :::  single tournament update");
                });


                formant = JsonUtility.FromJson<APIFormant>(body);
                formant.ParseBody();
                output = formant.response.parse.data;
                action.Invoke(true,body);
                return;
            }
            action.Invoke(false, error);

        });

        }


    public void CancelTournamentTicket(string ticket_id,string tournament_id, string session_token, string userToken, string currency, Action<bool, string> action)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();

        parameters.Add("request_type", "cancelBet");
        parameters.Add("user_id", userID);
        parameters.Add("session_token", session_token);
        parameters.Add("user_token", userToken);
        parameters.Add("id", ticket_id);
        parameters.Add("is_bot", "0");
        parameters.Add("currency", currency);
        parameters.Add("platform", JsonUtility.ToJson(new DeviceInfo()));

        string body = JsonConvert.SerializeObject(parameters);
        WebApiManager.Instance.GetNetWorkCallPostMethodUsingJson(APIURL, body, (success, error, body) =>
        {
            if (success)
            {
                FetchTournament(APIURL, userID, tournament_id, (success, updatedtournament) => {
                    int index = TournamentList.FindIndex(x => x.tournament_id == updatedtournament.tournament_id);

                    TournamentList[index].round_data = updatedtournament.round_data;
                    if (TournamentList[index].round_data[1].status == TournamentStatus.End)
                    {
                        TournamentList[index].status = TournamentStatus.End;
                    }
                    TournamentList[index].action?.Invoke(TournamentList[index]);

                    Debug.Log("invoke tournamnet changes :::  single tournament update");
                });
                formant = JsonUtility.FromJson<APIFormant>(body);
                formant.ParseBody();
                output = formant.response.parse.data;
                action.Invoke(true, body);
                return;
            }
            action.Invoke(false, error);

        });

    }

    public async void FetchTournamentData(string user_id, Action<bool,string> action)
    {

        // Start the scheduler



        bool IsBetlistFound = false;
        bool IsBetlistRecived = false;
        betHistory = new BetHistoryController();
        betHistory.GetTournamentBetList(APIURL, user_id, (success) =>
        {
            IsBetlistRecived = true;
            IsBetlistFound = success;
        });
        bool IsTournamentlistFound = false;
        bool IsTournamentlistRecived = false;

        FetchTournament(APIURL, user_id, (success) =>
        {
            IsTournamentlistRecived = true;
            IsTournamentlistFound = success;

        });

        while (!IsTournamentlistRecived || !IsBetlistRecived)
        {
            await Task.Delay(500);
        }
        if (IsBetlistFound && IsTournamentlistFound)
        {
            action.Invoke(true, "data fetch successfully");
            return;
        }

        string message = "";
        if (!IsBetlistFound)
            message += "error while fetch bet list.";
        if (!IsTournamentlistFound)
            message += "error while fetch tournament list.";
        action.Invoke(false, message);

    }

    

    public void ValidateTournament(DateTime dateTime)
    {


        var t_list = from tournament in TournamentList where (TimerHelper.UtcStringToDateTimeUtc(tournament.round_data[0].closed_at) <= dateTime && tournament.round_data[0].status == TournamentStatus.Live) || (TimerHelper.UtcStringToDateTimeUtc(tournament.round_data[1].closed_at) <= dateTime && tournament.round_data[1].status == TournamentStatus.Live) || (TimerHelper.UtcStringToDateTimeUtc(tournament.round_data[0].start_at) <= dateTime && tournament.round_data[0].status == TournamentStatus.Upcoming) || (TimerHelper.UtcStringToDateTimeUtc(tournament.round_data[1].start_at) <= dateTime && tournament.round_data[1].status == TournamentStatus.Upcoming) || (TimerHelper.UtcStringToDateTimeUtc(tournament.round_data[0].result_at) <= dateTime && tournament.round_data[0].status == TournamentStatus.Running) || (TimerHelper.UtcStringToDateTimeUtc(tournament.round_data[1].result_at) <= dateTime && tournament.round_data[1].status == TournamentStatus.Running) select tournament;

        var tournament_list = t_list.ToList();
        Debug.Log("validate tournament " + dateTime+"current tournament list is " + tournament_list.Count + " :::: data is :: " + JsonConvert.SerializeObject(tournament_list));

        output = JsonConvert.SerializeObject(tournament_list);

        foreach(var tournament in tournament_list)
        {
            FetchTournament(APIURL, userID, tournament.tournament_id, (success, updatedtournament) => {
                int index = TournamentList.FindIndex(x=>x.tournament_id == updatedtournament.tournament_id);

                TournamentList[index].round_data = updatedtournament.round_data;
                if (TournamentList[index].round_data[1].status == TournamentStatus.End)
                {
                    TournamentList[index].status = TournamentStatus.End;
                }
                TournamentList[index].action?.Invoke(TournamentList[index]);

                Debug.Log("invoke tournamnet changes :::  single tournament update");
            });
        }

        TournamentModel nextTournament = TournamentList
    .Where(t => t.round_data[1].status == TournamentStatus.Live)
    .OrderBy(t => t.round_data[1].closed_time)
    .FirstOrDefault();

        int nextTournamentIndex = TournamentList.FindIndex(x=>x.tournament_id == nextTournament.tournament_id);

        if (TournamentList[nextTournamentIndex].status != TournamentStatus.Live)
        {
            TournamentList[nextTournamentIndex].status = TournamentStatus.Live;
            TournamentList[nextTournamentIndex].action.Invoke(TournamentList[nextTournamentIndex]);
        }


    }





    void FetchTournament(string url, string user_id, Action<bool> action)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();

        parameters.Add("request_type", "get_tournaments");
        parameters.Add("user_id", user_id);

        string body = JsonConvert.SerializeObject(parameters);
        WebApiManager.Instance.GetNetWorkCallPostMethodUsingJson(url, body, (success, error, body) => {
            if (success)
            {
                formant = JsonUtility.FromJson<APIFormant>(body);
                formant.ParseBody();
                output = formant.response.parse.data;
                TournamentList = JsonConvert.DeserializeObject<List<TournamentModel>>(formant.response.parse.data);
                TournamentList = TournamentList.OrderBy(x => x.round_data[0].closed_at).ToList();
                List<string> removeList = new List<string>();
                foreach (var tournament in TournamentList)
                {
                    tournament.ParseBody();
                    TournamentManager.instance.OnUpdateSingleTournamentUpdate.Add(tournament.tournament_id, tournament.action);
                    if (tournament.round_data.Count != 2)
                    {
                        Debug.Log($"check this tournament with backend team... tournament_id  {tournament.tournament_id}   tournament_name  {tournament.tournament_name}  reson tournament round count is {tournament.round_data.Count}");
                        removeList.Add(tournament.tournament_name);
                    }
                }

                foreach (string data in removeList)
                {
                    int index = TournamentList.FindIndex(x => x.tournament_name == data);
                    if (index != -1)
                    {
                        TournamentList.RemoveAt(index);
                    }
                }

                TournamentModel nextTournament = TournamentList
                .Where(t => t.round_data[1].status == TournamentStatus.Live)
                .OrderBy(t => t.round_data[1].closed_time)
                .FirstOrDefault();

                int nextTournamentIndex = TournamentList.FindIndex(x => x.tournament_id == nextTournament.tournament_id);

                if (TournamentList[nextTournamentIndex].status != TournamentStatus.Live)
                {
                    TournamentList[nextTournamentIndex].status = TournamentStatus.Live;
                }
            }
            else
            {
                Debug.Log("failed to load tournament");

            }
            action.Invoke(success);

        }
        );
    }

    void FetchTournament(string url, string user_id, string tournament_id, Action<bool,TournamentModel> action)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();

        parameters.Add("request_type", "get_tournaments");
        parameters.Add("user_id", user_id);
        parameters.Add("tournament_id", tournament_id);
        

        string body = JsonConvert.SerializeObject(parameters);
        WebApiManager.Instance.GetNetWorkCallPostMethodUsingJson(url, body, (success, error, body) => {
            if (success)
            {
                formant = JsonUtility.FromJson<APIFormant>(body);
                formant.ParseBody();
                output = formant.response.parse.data;
                var tempTournament = JsonConvert.DeserializeObject<List<TournamentModel>>(formant.response.parse.data);

                if(tempTournament.Count == 1)
                {
                    tempTournament[0].ParseBody();
                    action.Invoke(success, tempTournament[0]);
                }
                else
                {
                    Debug.Log($"check this tournament with backend team... tournament_id  {tournament_id} reson  request single tournament but recive multiple or emety response");
                }
                tempTournament = null;

            }
            else
            {
                Debug.Log("failed to load tournament");

            }

        }
        );
    }




}
