namespace Divisas2.Services
{
    using System;
	using System.Net.Http;
	using System.Threading.Tasks;
	using Divisas2.Models;
	using Newtonsoft.Json;
    using Plugin.Connectivity;

    public class ApiService
    {
		public async Task<Response> CheckConnection()
		{
			if (!CrossConnectivity.Current.IsConnected)
			{
				return new Response
				{
					IsSuccess = false,
					Message = "Please turn on your internet settings.",
				};
			}

			var isReachable = await CrossConnectivity.Current.IsRemoteReachable("google.com");
			if (!isReachable)
			{
				return new Response
				{
					IsSuccess = false,
					Message = "Check you internet connection.",
				};
			}

			return new Response
			{
				IsSuccess = true,
				Message = "Ok",
			};
		}		

        public async Task<Response> Get<T>(string urlBase, string url)
		{
			try
			{
				var client = new HttpClient();
				client.BaseAddress = new Uri(urlBase);
				var response = await client.GetAsync(url);

				if (!response.IsSuccessStatusCode)
				{
					return new Response
					{
						IsSuccess = false,
						Message = response.StatusCode.ToString(),
					};
				}

				var result = await response.Content.ReadAsStringAsync();
				var serialized = JsonConvert.DeserializeObject<T>(result);
				return new Response
				{
					IsSuccess = true,
					Message = "Ok",
					Result = serialized,
				};
			}
			catch (Exception ex)
			{
				return new Response
				{
					IsSuccess = false,
					Message = ex.Message,
				};
			}
		}
	}
}
