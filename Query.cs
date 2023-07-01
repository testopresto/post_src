using Npgsql;
using System.Collections.Generic;
using System.Text.Json;
static class Query{
	public static int Insert(Dictionary<string, Mapping> mappings, JsonDocument doc, string dbConfig){
        int objectCount = 0;
        int paramCount = 1;
        using (var connection = new NpgsqlConnection(dbConfig))
        {
            using (var cmd = new NpgsqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText = "INSERT INTO data (" + string.Join(',', mappings.Select(_ => _.Value.TableColumn)) + ") VALUES ";
                var commandLines = new List<string>();
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    var sqlParams = new List<NpgsqlParameter>();
                    var row = item.EnumerateObject().ToDictionary(_ => _.Name);
                    objectCount++;
                    foreach (var mapping in mappings)
                    {
                        var column = row[mapping.Key];
                        var param = new NpgsqlParameter();
                        param.ParameterName = (paramCount++).ToString();
                        switch (mapping.Value.ColumnType)
                        {
                            case "integer":
                                param.Value = column.Value.GetInt32();
                                param.DataTypeName = "int";
                                break;
                            case "money":
                                param.Value = column.Value.GetDecimal();
                                param.DataTypeName = "numeric";
                                break;
                            default:
                                param.Value = column.Value.GetString();
                                param.DataTypeName = "varchar";
                                break;
                        }
                        sqlParams.Add(param);
                        cmd.Parameters.Add(param);
                    }
                    commandLines.Add("(" + string.Join(',', sqlParams.Select(_ => "@" + _.ParameterName)) + ")");
                }
                cmd.CommandText += string.Join(',', commandLines);
                connection.Open();
                cmd.ExecuteNonQuery();
                return objectCount;
            }
        }
    }
}