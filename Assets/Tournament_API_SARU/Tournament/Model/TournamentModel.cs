using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;

[System.Serializable]

public class TournamentModel
{
    public TournamentStatus status;

    public string id;
    public string tournament_name;
    public int entry_fee;
    public string tournament_id;
    public string match_token;
    public string game_name;
    public Action<TournamentModel> action;
    [SerializeField]
    public List<TournamentRoundModel> round_data = new List<TournamentRoundModel>();
    public void ParseBody()
    {        
        foreach (var tournamentRound in round_data)
        {
            tournamentRound.ParseBody();
        }
        if (round_data[0].status == TournamentStatus.End)
        {
            status = TournamentStatus.End;
        }
        else
        {
            status = TournamentStatus.Upcoming;
        }
    }
}
[System.Serializable]

public class TournamentRoundModel
{
    [SerializeField]

    public TournamentStatus status;
    public string start_time;
    public string closed_time;
    public string result_time;
    public string start_at;
    public string closed_at;
    public string result_at;
    public string round_name;
    public string result_data;
    public string ticket_data;
    public string ticket_id;
    private List<TicketList> tickets = new List<TicketList>();
    [SerializeField]
    public TicketList ticket = new TicketList();

    public void ParseBody()
    {
        start_time = TimerHelper.UtcStringToDateTimeIst(start_at).ToString("dd/MM/yyyy  hh:mm tt");
        closed_time = TimerHelper.UtcStringToDateTimeIst(closed_at).ToString("dd/MM/yyyy  hh:mm tt");
        result_time = TimerHelper.UtcStringToDateTimeIst(result_at).ToString("dd/MM/yyyy  hh:mm tt");

        if (ticket_data == null)
            ticket_data = "";
        tickets = JsonConvert.DeserializeObject<List<TicketList>>(ticket_data);
        if(ticket_data.Length > 20 && tickets.Count == 3)
        {
            ticket.direct_number = tickets[0].direct_number ?? tickets[1].direct_number ?? tickets[2].direct_number;
            ticket.ending = tickets[0].ending ?? tickets[1].ending ?? tickets[2].ending;
            ticket.housing = tickets[0].housing ?? tickets[1].housing ?? tickets[2].housing;
        }

    }

}
[System.Serializable]

public enum TournamentStatus{
    Upcoming = 0,
    Live = 1,
    Running = 2,
    End = 3
}

[System.Serializable]

public class BetHistoryModel
{
    public string id;
    public string tournament_id;
    public string round_id;
    public string user_id;
    public int entry_amount;
    public string bet_id;
    public int winning_amount;
    public string winning_data;
    public string created_at;
    public string ticket_id;
    public string ticket_data;
    public string start_time;
    public string closed_time;
    public string result_time;

    public string tournament_name;
    public string start_at;
    public string closed_at;
    public string result_at;
    public string game_name;
    public string player_name;
    private List<TicketList> tickets = new List<TicketList>();
    [SerializeField]
    public TicketList ticket = new TicketList();

    public void ParseBody()
    {
        start_time = TimerHelper.UtcStringToDateTimeIst(start_at).ToString("hh:mm tt");
        closed_time = TimerHelper.UtcStringToDateTimeIst(start_at).ToString("hh:mm tt");
        result_time = TimerHelper.UtcStringToDateTimeIst(start_at).ToString("hh:mm tt");
        if (ticket_data == null)
            ticket_data = "";
        tickets = JsonConvert.DeserializeObject<List<TicketList>>(ticket_data);
        if (ticket_data.Length > 20 && tickets.Count == 3)
        {
            ticket.direct_number = tickets[0].direct_number ?? tickets[1].direct_number ?? tickets[2].direct_number;
            ticket.ending = tickets[0].ending ?? tickets[1].ending ?? tickets[2].ending;
            ticket.housing = tickets[0].housing ?? tickets[1].housing ?? tickets[2].housing;

            ticket.total_count = ticket.direct_number.Count + ticket.ending.Count + ticket.housing.Count;
        }

    }
}

[System.Serializable]

public class TicketInfo
{
    public int value;
    public int amount;
    public string created_at;
    public string updated_at;
}
[System.Serializable]

public class TicketList
{
    [SerializeField]

    public List<TicketInfo> ending;
    [SerializeField]
    public List<TicketInfo> housing;
    [SerializeField]
    public List<TicketInfo> direct_number;
    public int total_count;
}
[System.Serializable]
public class APIFormant
{
    public bool success;
    [SerializeField]
    public APIResponse response;

    public void ParseBody()
    {
        response.ParseBody();
    }

}
[System.Serializable]

public class APIResponse
{
    public int statusCode;
    public string body;
    [SerializeField]
    public APIBody parse;

    public void ParseBody()
    {
        parse = JsonUtility.FromJson<APIBody>(body);
    }
}
[System.Serializable]

public class APIBody
{
    public string code;
    public string message;
    public string data;
}

[System.Serializable]

public class DeviceInfo
{
    public string device_name;
    public string device_model;
    public string device_id;
    public string device_type;
    public string processor;
    public string os;
    public DeviceInfo()
    {
        device_id = SystemInfo.deviceUniqueIdentifier;
        device_name = SystemInfo.deviceName;
        device_model = SystemInfo.deviceModel;
        device_type = SystemInfo.deviceType.ToString();
        processor = SystemInfo.processorType;
        os = SystemInfo.operatingSystem;
    }
}

