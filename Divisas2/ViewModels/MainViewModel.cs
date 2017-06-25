namespace Divisas2.ViewModels
{
    using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.ComponentModel;
	using System.Reflection;
	using System.Windows.Input;
	using Divisas2.Models;
	using Divisas2.Services;
	using GalaSoft.MvvmLight.Command;
	using System.Linq;
    using System.Threading.Tasks;

    public class MainViewModel : INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Attributes
        ApiService apiService;
        DialogService dialogService;
        DataService dataService;
        bool isRunning;
        bool isEnabled;
        string message;
        string sourceRate;
        string targetRate;
        string status;
		ExchangeRates exchangeRates;
		ExchangeNames exchangeNames;
        List<Rate> rates;
		#endregion

		#region Properties
        public ObservableCollection<Rate> Rates 
        { 
            get; 
            set; 
        }

        public bool IsRunning
        {
            set
            {
                if (isRunning != value)
                {
                    isRunning = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
                }
            }
            get
            {
                return isRunning;
            }
        }

        public bool IsEnabled
        {
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsEnabled"));
                }
            }
            get
            {
                return isEnabled;
            }
        }

		public string Message
		{
			set
			{
				if (message != value)
				{
					message = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Message"));
				}
			}
			get
			{
				return message;
			}
		}

		public string Status
		{
			set
			{
				if (status != value)
				{
					status = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
				}
			}
			get
			{
				return status;
			}
		}

		public decimal Amount
        {
            get;
            set;
        }

        public string SourceRate
        {
			set
			{
				if (sourceRate != value)
				{
					sourceRate = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SourceRate"));
				}
			}
			get
			{
				return sourceRate;
			}
		}

        public string TargetRate
        {
			set
			{
				if (targetRate != value)
				{
					targetRate = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TargetRate"));
				}
			}
			get
			{
				return targetRate;
			}
		}

        #endregion

        #region Constructors
        public MainViewModel()
        {
            Status = "Cargando tasas...";

            apiService = new ApiService();
            dialogService = new DialogService();
            dataService = new DataService();

            Rates = new ObservableCollection<Rate>();

            GetRates();
        }
        #endregion

        #region Methods
        async void GetRates()
        {
			IsRunning = true;
			IsEnabled = false;

			var checkConnetion = await apiService.CheckConnection();
			if (checkConnetion.IsSuccess)
			{
                await GetRatesFromAPI();
                SaveRates();
			}
            else
            {
                GetRatesFromData();
				Status = "Tasas cargadas localmente.";
			}

			var lastQuery = dataService.First<LastQuery>(false);
			if (lastQuery != null)
			{
				SourceRate = lastQuery.CodeRateSource;
				TargetRate = lastQuery.CodeRateTarget;
			}
			
            IsRunning = false;
			IsEnabled = true;
		}

        void SaveRates()
        {
            dataService.DeleteAll<Rate>();      
            dataService.Save(rates);
        }

        void GetRatesFromData()
        {
            rates = dataService.Get<Rate>(false);
			Rates.Clear();
			foreach (var rate in rates)
			{
				Rates.Add(new Rate
				{
					Code = rate.Code,
					Name = rate.Name,
					TaxRate = rate.TaxRate,
				});
			}
		}

        async Task GetRatesFromAPI()
        {
			var response1 = await apiService.Get<ExchangeRates>(
				"https://openexchangerates.org",
				"/api/latest.json?app_id=f490efbcd52d48ee98fd62cf33c47b9e");

			var response2 = await apiService.Get<ExchangeNames>(
				"https://gist.githubusercontent.com",
				"/picodotdev/88512f73b61bc11a2da4/raw/9407514be22a2f1d569e75d6b5a58bd5f0ebbad8");

			if (response1.IsSuccess && response2.IsSuccess)
			{
				exchangeRates = (ExchangeRates)response1.Result;
				exchangeNames = (ExchangeNames)response2.Result;
				LoadRates();
				Status = "Tasas cargadas de internet.";
			}
            else
            {
				Status = "Ocurrio un problema cargando las tasas, por favor intente más tarde.";
			}
        }

        void LoadRates()
        {
            // Get values
			var rateValues = new List<RateValue>();
			var type = typeof(Rates);
			var properties = type.GetRuntimeFields();

			foreach (var property in properties)
			{
				var code = property.Name.Substring(1, 3);
				rateValues.Add(new RateValue
				{
					Code = code,
					TaxRate = (double)property.GetValue(exchangeRates.Rates),
				});
			}

            // Get names
			var rateNames = new List<RateName>();
            type = typeof(ExchangeNames);
			properties = type.GetRuntimeFields();

			foreach (var property in properties)
			{
				var code = property.Name.Substring(1, 3);
                rateNames.Add(new RateName
				{
					Code = code,
                    Name = (string)property.GetValue(exchangeNames),
				});
			}

            // Join the complete list
            var qry = (from v in rateValues
                       join n in rateNames on v.Code equals n.Code
                       select new { v, n }).ToList();

            Rates.Clear();
            rates = new List<Rate>();
            foreach (var item in qry)
			{
                Rates.Add(new Rate 
                {
                    Code = item.v.Code,
                    Name = item.n.Name,
                    TaxRate = item.v.TaxRate,
                });

				rates.Add(new Rate
				{
					Code = item.v.Code,
					Name = item.n.Name,
					TaxRate = item.v.TaxRate,
				});
			}
        }
        #endregion

        #region Commands
        public ICommand ChangeCommand
        {
			get { return new RelayCommand(Change); }
		}

		private void Change()
        {
            var aux = SourceRate;
            SourceRate = TargetRate;
            TargetRate = aux;
            ConvertMoney();
        }

		public ICommand ConvertMoneyCommand
        {
            get { return new RelayCommand(ConvertMoney); }
        }

		private async void ConvertMoney()
		{
			if (Amount <= 0)
			{
				await App.Current.MainPage.DisplayAlert(
                    "Error", 
                    "Debes ingresar un valor a convertir", 
                    "Aceptar");
				return;
			}

            if (string.IsNullOrEmpty(SourceRate))
			{
				await App.Current.MainPage.DisplayAlert(
                    "Error", 
                    "Debes seleccionar la moneda origen", 
                    "Aceptar");
				return;
			}

			if (string.IsNullOrEmpty(TargetRate))
			{
				await App.Current.MainPage.DisplayAlert(
                    "Error", 
                    "Debes seleccionar la moneda destino", 
                    "Aceptar");
				return;
			}

            decimal amountConverted = Amount /
                                      Convert.ToDecimal(SourceRate.Substring(3)) *
                                      Convert.ToDecimal(TargetRate.Substring(3));

            Message = string.Format("{0} {1:N2} = {2} {3:N2}", 
                                    SourceRate.Substring(0, 3),
                                    Amount,
									TargetRate.Substring(0, 3),
									amountConverted);

            var lastQuery = new LastQuery
            {
                CodeRateSource = SourceRate,
                CodeRateTarget = TargetRate,
            };

            dataService.DeleteAll<LastQuery>();
            dataService.Insert(lastQuery);
		}
		#endregion
	}
}
