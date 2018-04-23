using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace cem_updater_core.DAL
{
    public  static class KillBoard
    {


        public static bool AddWaiting(Model.Esi_war_kms model, bool tq = false)
        {
            using (var conn = new NpgsqlConnection(Helpers.GetKBConnString(tq)))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO killboard_waiting_api (\"killID\", hash) VALUES (@id,@hash);";
                    cmd.Parameters.AddWithValue("id", model.killmail_id);
                    cmd.Parameters.AddWithValue("hash", model.killmail_hash);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        public static bool AddWaiting(List<Model.Esi_war_kms> model, bool tq = false)
        {
            Parallel.ForEach(model, new ParallelOptions() { MaxDegreeOfParallelism = 10 }, m => { AddWaiting(m, tq); });
            return true;

        }

        public static bool AddWaiting(Model.Kb_waiting_api model, bool tq = false)
        {
            using (var conn=new NpgsqlConnection(Helpers.GetKBConnString(tq)))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO killboard_waiting_api (\"killID\", hash) VALUES (@id,@hash);";
                    cmd.Parameters.AddWithValue("id", model.killID);
                    cmd.Parameters.AddWithValue("hash", model.hash);
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        public static bool AddWaiting(List<Model.Kb_waiting_api> model, bool tq = false)
        {
            Parallel.ForEach(model, new ParallelOptions() {MaxDegreeOfParallelism = 10}, m => { AddWaiting(m, tq); });
            return true;

        }

        public static bool UpdateWar(Model.Kb_war model, bool tq = false)
        {
            using (var conn=new NpgsqlConnection(Helpers.GetKBConnString(tq)))
            {
                conn.Open();
                using (var cmd=new NpgsqlCommand())
                {
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
                    cmd.ExecuteNonQuery();
                    return true;


                }
            }

        }

        public static Dictionary<int,int[]> GetWarStatus(bool tq = false)
        {
            Dictionary<int, int[]> result=new Dictionary<int, int[]>();
            using (var conn = new NpgsqlConnection(Helpers.GetKBConnString(tq)))
            {
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT \"warID\", finished,lastkm FROM killboard_war";
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(reader.GetInt32(0),new []
                            {
                                reader.IsDBNull(1)?0:(reader.GetBoolean(1)?1:0),
                                reader.IsDBNull(2)?0:reader.GetInt32(2)

                            } );
                        }
                    }
                }
            }

            return result;
            
        }
    }
}
