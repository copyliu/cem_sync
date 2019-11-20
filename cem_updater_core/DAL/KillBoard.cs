using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dasync.Collections;
using Npgsql;

namespace cem_updater_core.DAL
{
    public  static class KillBoard
    {
        private static async Task<bool> AddWaiting(Model.Esi_war_kms model, bool tq = false)
        {
            await using var conn = new NpgsqlConnection(Helpers.GetKBConnString(tq));
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "INSERT INTO killboard_waiting_api (\"killID\", hash) VALUES (@id,@hash);";
            cmd.Parameters.AddWithValue("id", model.killmail_id);
            cmd.Parameters.AddWithValue("hash", model.killmail_hash);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }

        public static async Task<bool> AddWaiting(List<Model.Esi_war_kms> model, bool tq = false)
        {
            await model.ParallelForEachAsync(async kms => await AddWaiting(kms, tq));

           
            return true;

        }

        public static async Task<bool> AddWaiting(Model.Kb_waiting_api model, bool tq = false)
        {
            await using var conn=new NpgsqlConnection(Helpers.GetKBConnString(tq));
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "INSERT INTO killboard_waiting_api (\"killID\", hash) VALUES (@id,@hash);";
            cmd.Parameters.AddWithValue("id", model.killID);
            cmd.Parameters.AddWithValue("hash", model.hash);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }

        public static async Task<bool> AddWaiting(List<Model.Kb_waiting_api> model, bool tq = false)
        {
            await model.ParallelForEachAsync(async kms => await AddWaiting(kms, tq));


            return true;

        }

        public static async Task<bool> UpdateWar(Model.Kb_war model, bool tq = false)
        {
            await using var conn=new NpgsqlConnection(Helpers.GetKBConnString(tq));
            await conn.OpenAsync();
            await using var cmd=new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandText =
                " INSERT INTO killboard_war (\"warID\", rawdata, finished, lastkm, cacheutil) VALUES (@warid,@rawdata,@finished,@lastkm,@cacheutil) " +
                " ON CONFLICT DO UPDATE SET" +
                " rawdata=@rawdata, finished=@finished, lastkm=@lastkm,cacheutil=@cacheutil;";
            cmd.Parameters.AddWithValue("warid", model.warID);
            cmd.Parameters.AddWithValue("rawdata", model.rawdata);
            cmd.Parameters.AddWithValue("finished", model.finished);
            cmd.Parameters.AddWithValue("lastkm", model.lastkm);
            cmd.Parameters.AddWithValue("cacheutil", model.cacheutil);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }

        public static async Task<Dictionary<int, int[]>> GetWarStatus(bool tq = false)
        {
            Dictionary<int, int[]> result = new Dictionary<int, int[]>();
            await using var conn = new NpgsqlConnection(Helpers.GetKBConnString(tq));
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = "SELECT \"warID\", finished,lastkm FROM killboard_war";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(reader.GetInt32(0), new[]
                {
                    reader.IsDBNull(1) ? 0 : (reader.GetBoolean(1) ? 1 : 0),
                    reader.IsDBNull(2) ? 0 : reader.GetInt32(2)

                });
            }


            return result;

        }
    }
}
