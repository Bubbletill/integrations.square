using BT_COMMONS.App;
using BT_COMMONS.Integrations.Square;
using Newtonsoft.Json;
using Square;
using Square.Apis;
using Square.Exceptions;
using Square.Models;

namespace BT_INTEGRATIONS.SQUARE;

public class SquareHandlers
{

    public static SquareClient SquareClient = new SquareClient.Builder()
    .Environment(Square.Environment.Production)
    .BearerAuthCredentials(
        new Square.Authentication.BearerAuthModel.Builder(Program.API).Build())
    .Build();

    public static async Task<string?> GetDeviceCode()
    {
        // Load terminal device ID
        try
        {
            using (StreamReader r = new StreamReader("C:\\bubbletill\\sqaure-integration.json"))
            {
                string json = r.ReadToEnd();
                SquareDeviceData data = JsonConvert.DeserializeObject<SquareDeviceData>(json);

                if (data.terminal_device_code == null || data.terminal_device_code.Length == 0)
                {
                    throw new Exception();
                }

                return data.terminal_device_code;
            }
        }
        catch (Exception ex)
        {
            return await GenerateNewDeviceCode();
        }
    }

    public static async Task<string?> GenerateNewDeviceCode()
    {
        var StoreNumber = 0;
        var RegisterNumber = 0;
        // Load data.json
        try
        {
            using (StreamReader r = new StreamReader("C:\\bubbletill\\data.json"))
            {
                string json = r.ReadToEnd();
                AppConfig config = JsonConvert.DeserializeObject<AppConfig>(json);

                if (config == null || config.Register == null || config.Store == null)
                {
                    throw new Exception();
                }

                StoreNumber = (int)config.Store;
                RegisterNumber = (int)config.Register;
            }
        }
        catch (Exception ex)
        {
            return null;
        }

        CreateDeviceCodeRequest body = new CreateDeviceCodeRequest.Builder(
            Guid.NewGuid().ToString(),
            new DeviceCode.Builder(
                "TERMINAL_API"
            )
            .Name("BT Terminal Store " + StoreNumber + " Register " + RegisterNumber)
            .Build()
            )
        .Build();


        try
        {
            CreateDeviceCodeResponse result = await SquareClient.DevicesApi.CreateDeviceCodeAsync(body);


            SquareDeviceData deviceData = new SquareDeviceData();
            deviceData.api_key = Program.API;
            deviceData.terminal_device_code = result.DeviceCode.Code;
            deviceData.terminal_device_id = result.DeviceCode.DeviceId;
            string json = JsonConvert.SerializeObject(deviceData);
            File.WriteAllText("C:\\bubbletill\\sqaure-integration.json", json);

            return result.DeviceCode.Code;
        }
        catch (ApiException e)
        {
            return null;
        }
    }

    public async static Task<string?> CheckPairStatus(string deviceCode)
    {
        try
        {
            GetDeviceCodeResponse result = await SquareClient.DevicesApi.GetDeviceCodeAsync(deviceCode);
            return result.DeviceCode.Status;
        }
        catch (ApiException e)
        {
            return "ERROR";
        }
    }

}
