//////////////////////////////////////////////
// MIT - 2004-2019
// Author : Derek Tremblay (derektremblay666@gmail.com)
//////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MaskedTextBox
{
	public class MaskedTextBox : TextBox
	{

        #region Global variables
        private Mask _mask;
		private int _digitPos;
		private int _delimitNumber;
		private string _dateFormat = "dd/mm/yyyy";
		private ErrorProvider _errorProvider;
		private Container _components;
		private int _minimalYear = 1900;
		private int _maxYear = 2100;

		private static readonly ResourceManager _resourceManager =
			new ResourceManager("MaskedTextBox.MaskEditRes", typeof(MaskedTextBox).Assembly);
        #endregion

        #region Constructor
        public MaskedTextBox()
		{
			InitializeComponent();

			if (Masked != Mask.None)
				_mask = Masked;
		}
        #endregion

        public Mask Masked
		{
			get => _mask;
			set
			{
				_mask = value;
				Text = string.Empty;
				switch (_mask)
				{
					case Mask.DateOnly:
						MaxLength = 10;
						break;
					case Mask.IpAddress:
						MaxLength = 15;
						break;
					case Mask.PhoneWithArea:
						MaxLength = 12;
						break;
					case Mask.SSN:
						MaxLength = 11;
						break;
				}
			}
		}

		/// <summary>
		/// The control supports only short date formatting - Possible values: dd/mm/yyyy and mm/dd/yyyy
		/// </summary>
		public FormatDate DateFormat
		{
			get => "dd/mm/yyyy" == _dateFormat 
				? FormatDate.ddmmyyyy 
				: FormatDate.mmddyyyy;

			set => _dateFormat = FormatDate.ddmmyyyy == value 
				? "dd/mm/yyyy" 
				: "mm/dd/yyyy";
		}


		/// <summary>
		/// Minimal year
		/// </summary>
		public int MinYear
		{
			get => _minimalYear;
			set
			{
				if (value >= DateTime.MinValue.Year)
					_minimalYear = value;
			}
		}

		/// <summary>
		/// Maximal year
		/// </summary>
		public int MaxYear
		{
			get => _maxYear;
			set
			{
				if (value <= DateTime.MaxValue.Year)
					_maxYear = value;
			}
		}
		public string Error => _errorProvider.GetError(this);

		private static string GetStr(string key) => _resourceManager.GetString(key);

		#region Component Designer generated code
		private void InitializeComponent()
		{
			_components = null;
			_digitPos = 0;
			_delimitNumber = 0;

			_errorProvider = new ErrorProvider
			{
				BlinkStyle = ErrorBlinkStyle.NeverBlink
			};

			KeyPress += new KeyPressEventHandler(OnKeyPress);
			Leave += new EventHandler(OnLeave);
		}
		#endregion

		private void OnKeyPress(object sender, KeyPressEventArgs e)
		{
			var sd = (MaskedTextBox)sender;
			switch (_mask)
			{
				case Mask.DateOnly:
					if ("dd/mm/yyyy" == _dateFormat)
						sd.MaskDate_ddmmyyyy(e);
					else
						sd.MaskDate_mmddyyyy(e);
					break;
				case Mask.PhoneWithArea:
					sd.MaskPhoneSSN(e, 3, 3);
					break;
				case Mask.IpAddress:
					sd.MaskIpAddr(e);
					break;
				case Mask.SSN:
					sd.MaskPhoneSSN(e, 3, 2);
					break;
				case Mask.Decimal:
					sd.MaskDecimal(e);
					break;
				case Mask.Digit:
					sd.MaskDigit(e);
					break;
			}
		}
		private void OnLeave(object sender, EventArgs e)
		{
			var sd = (MaskedTextBox)sender;
			Regex regStr;

			switch (_mask)
			{
				case Mask.DateOnly:
					if (TextLength > 0)
					{
						regStr = new Regex(@"\d{2}/\d{2}/\d{4}");
						if (!regStr.IsMatch(sd.Text))
						{
							if ("dd/mm/yyyy" == _dateFormat)
								_errorProvider.SetError(this, GetStr("DATEFORMATddmmyyyy"));
							else
								_errorProvider.SetError(this, GetStr("DATEFORMAT"));
						}

						try
						{
							DateTime d = DateTime.Parse(Text);
							if (d.Year < _minimalYear || _maxYear < d.Year)
								_errorProvider.SetError(this, GetStr("YEARBETWEEN"));
						}
						catch (FormatException)
						{
							if ("dd/mm/yyyy" == _dateFormat)
								_errorProvider.SetError(this, GetStr("DATEFORMATddmmyyyy"));
							else
								_errorProvider.SetError(this, GetStr("DATEFORMAT"));
						}
					}
					else
						_errorProvider.SetError(this, string.Empty);
					break;
				case Mask.PhoneWithArea:
					regStr = new Regex(@"\d{3}-\d{3}-\d{4}");

					if (!regStr.IsMatch(sd.Text))
						_errorProvider.SetError(this, GetStr("PHONEFORMAT"));

					break;
				case Mask.IpAddress:
					short cnt = 0;
					var len = sd.Text.Length;

					for (short i = 0; i < len; i++)
						if (sd.Text[i] == '.')
						{
							cnt++;
							if (i + 1 < len)
								if (sd.Text[i + 1] == '.')
								{
									_errorProvider.SetError(this, GetStr("IP_FORMAT"));
									break;
								}
						}

					if (cnt < 3 || sd.Text[len - 1] == '.')
						_errorProvider.SetError(this, GetStr("IP_FORMAT"));

					break;
				case Mask.SSN:
					regStr = new Regex(@"\d{3}-\d{2}-\d{4}");
					if (!regStr.IsMatch(sd.Text))
						_errorProvider.SetError(this, GetStr("SSNFORMAT"));
					break;
				case Mask.Decimal:
					break;
				case Mask.Digit:
					break;
			}
		}
		private void MaskDigit(KeyPressEventArgs e)
		{
			if (char.IsDigit(e.KeyChar) || e.KeyChar == 8)
			{
				_errorProvider.SetError(this, string.Empty);
				e.Handled = false;
			}
			else
			{
				e.Handled = true;
				_errorProvider.SetError(this, GetStr("ONLYDIGIT"));
			}
		}
		private void MaskDecimal(KeyPressEventArgs e)
		{
			if (char.IsDigit(e.KeyChar) || e.KeyChar == '.' || e.KeyChar == 8)
			{
				e.Handled = false;
				_errorProvider.SetError(this, string.Empty);
			}
			else
			{
				e.Handled = true;
				_errorProvider.SetError(this, GetStr("ONLYDIGITANDDOT"));
			}
		}


		/// <summary>
		///  Support for min/max year in this format are not implemented
		/// </summary>
		private void MaskDate_ddmmyyyy(KeyPressEventArgs e)
		{
			_errorProvider.SetError(this, string.Empty);

			if (!char.IsDigit(e.KeyChar) && e.KeyChar != '/' && e.KeyChar != 8)
			{
				_errorProvider.SetError(this, GetStr("ONLYDIGITANDSLASH"));
				
				if (SelectedText == Text)
					Text = string.Empty;

				e.Handled = true;
			}
			else if (e.KeyChar != 8)
			{
				if (TextLength == MaxLength)
				{
					e.Handled = true;
					return;
				}

				string textValue;
				var noError = true;

				// if text is highlighted reset vars
				if (SelectedText == Text)
					textValue = e.KeyChar.ToString();
				else
				{
					if (SelectionStart == 0)
						textValue = e.KeyChar.ToString() + Text;
					else if (SelectionStart == TextLength)
						textValue = Text + e.KeyChar.ToString();
					else
						textValue = string.Format("{0}{1}{2}", Text.Substring(0, SelectionStart), e.KeyChar.ToString(), Text.Substring(SelectionStart, TextLength - SelectionStart));
				}

				int len;
				int indLastSlash;

				if (e.KeyChar == '/')
				{
					if (textValue.Length == 1 || textValue.Length == 4 || textValue.Length == 7)
					{
						_errorProvider.SetError(this, GetStr("ONLYDIGIT"));
						noError = false;
					}
					else if (textValue.Length == 2 || textValue.Length == 5)
					{
						textValue = textValue.Insert(textValue.Length - 2, "0");
						indLastSlash = textValue.LastIndexOf("/");

						if (0 == int.Parse(textValue.Substring(indLastSlash - 2, 2)))
						{
							_errorProvider.SetError(this, "This is not a valid day of month");
							noError = false;
						}
					}
				}
				else
				{
					len = textValue.Length;
					indLastSlash = textValue.LastIndexOf("/");

					switch (len)
					{
						case 2:
							if (int.Parse(textValue) > 31)
							{
								_errorProvider.SetError(this, string.Format("{0} 31", GetStr("NUMBERISSMALLERTHAN")));
								noError = false;
							}
							textValue += "/";
							break;
						case 4:
							Regex regStr = new Regex(@"\d{2}/\d{1}");
							if (!regStr.IsMatch(textValue))
							{
								_errorProvider.SetError(this, GetStr("DATEFORMATddmmyyyy"));
								noError = false;
							}
							break;
						case 3:
						case 6:
							textValue = textValue.Insert(textValue.Length - 1, "/");
							break;
						case 5:
							if (int.Parse(textValue.Substring(3, 2)) > 12)
							{
								_errorProvider.SetError(this, string.Format("{0} 12", GetStr("NUMBERISSMALLERTHAN")));
								noError = false;
							}
							else
								textValue += "/";
							break;
						case 10:
							int year = int.Parse(textValue.Substring(6, 4));
							if (year < _minimalYear || year > _maxYear)
							{
								_errorProvider.SetError(this, string.Format("{0}: {1:d}-{2:d}", GetStr("YEARBETWEEN"), _minimalYear, _maxYear));
								noError = false;
							}
							break;
					}
				}
				if (textValue.Length == 6 && !CheckDayOfMonth(int.Parse(textValue.Substring(3, 2)), int.Parse(textValue.Substring(0, 2))))
				{
					noError = false;
					_errorProvider.SetError(this, GetStr("DAYNOTVALID"));
				}

				if (noError)
					Text = textValue;

				SelectionStart = textValue.Length;
				e.Handled = true;
			}
		}

		private void MaskDate_mmddyyyy(KeyPressEventArgs e)
		{
			var len = Text.Length;
			var indx = Text.LastIndexOf("/");

			// if test is highlighted reset vars
			if (SelectedText == Text)
			{
				indx = -1;
				_digitPos = 0;
				_delimitNumber = 0;
				Text = null;
			}

			if (char.IsDigit(e.KeyChar) || e.KeyChar == '/' || e.KeyChar == 8)
			{
				string tmp = Text;
				if (e.KeyChar != 8)
				{
					if (TextLength == MaxLength)
					{
						e.Handled = true;
						return;
					}

					if (e.KeyChar != '/')
					{
						_digitPos = indx > 0 ? len - indx : _digitPos + 1;

						if (_digitPos == 3 && _delimitNumber < 2)
							if (e.KeyChar != '/')
							{
								_delimitNumber++;
								AppendText("/");
							}

						_errorProvider.SetError(this, "");
						if (_digitPos == 2 || (int.Parse(e.KeyChar.ToString()) > 1 && _delimitNumber == 0))
						{
							string tmp2 = indx == -1
								? e.KeyChar.ToString()
								: Text.Substring(indx + 1) + e.KeyChar.ToString();

							if (_delimitNumber < 2)
							{
								if (_digitPos == 1) AppendText("0");
								AppendText(e.KeyChar.ToString());

								if (indx < 0)
								{
									if (int.Parse(Text) > 12) // check validation
									{
										string str;
										str = Text.Insert(0, "0");

										if (int.Parse(Text) > 13)
										{
											Text = str.Insert(2, "/0");
											_delimitNumber++;
											AppendText("/");
										}
										else
										{
											Text = str.Insert(2, "/");
											AppendText("");
										}
										_delimitNumber++;
									}
									else
									{
										AppendText("/");
										_delimitNumber++;
									}
									e.Handled = true;
								}
								else
								{
									if (_delimitNumber == 1)
									{
										int m = int.Parse(Text.Substring(0, indx));
										if (!CheckDayOfMonth(m, int.Parse(tmp2)))
											_errorProvider.SetError(this, string.Format("{0} 31", GetStr("NUMBERISSMALLERTHAN")));
										else
										{
											AppendText("/");
											_delimitNumber++;
											e.Handled = true;
										}
									}
								}
							}
						}
						else if (_digitPos == 1 && int.Parse(e.KeyChar.ToString()) > 3 && _delimitNumber < 2)
						{
							if (_digitPos == 1) AppendText("0");
							AppendText(e.KeyChar.ToString());
							AppendText("/");
							_delimitNumber++;
							e.Handled = true;
						}
						else
						{
							if (_digitPos == 1 && _delimitNumber == 2 && e.KeyChar > '2')
								_errorProvider.SetError(this, "The year should start with 1 or 2");
						}
						if (_digitPos > 4)
							e.Handled = true;
					}
					else
					{
						_delimitNumber++;

						string tmp3 = indx == -1 ? Text.Substring(indx + 1) : Text;

						if (_digitPos == 1)
						{
							Text = tmp3.Insert(indx + 1, "0"); ;
							AppendText("/");
							e.Handled = true;
						}
					}
				}
				else
				{
					e.Handled = false;
					if ((len - indx) == 1)
					{
						_delimitNumber--;
						_digitPos = indx > -1 ? 2 : _digitPos - 1;
					}
					else
						_digitPos = indx > -1 ? len - indx - 1 : len - 1;
				}
			}
			else
			{
				e.Handled = true;
				_errorProvider.SetError(this, GetStr("ONLYDIGITANDSLASH"));
			}
		}
		private void MaskPhoneSSN(KeyPressEventArgs e, int pos, int pos2)
		{
			int len = Text.Length;
			int indx = Text.LastIndexOf("-");

			// if test is highlighted reset vars
			if (SelectedText == Text)
			{
				indx = -1;
				_digitPos = 0;
				_delimitNumber = 0;
			}

			if (char.IsDigit(e.KeyChar) || e.KeyChar == '-' || e.KeyChar == 8)
			{
				// only digit, Backspace and - are accepted
				string tmp = Text;
				if (e.KeyChar != 8)
				{
					if (TextLength == MaxLength)
					{
						e.Handled = true;
						return;
					}

					_errorProvider.SetError(this, string.Empty);
					if (e.KeyChar != '-')
						_digitPos = indx > 0 ? len - indx : _digitPos + 1;

					if (indx > -1 && _digitPos == pos2 && _delimitNumber == 1)
					{
						if (e.KeyChar != '-')
						{
							AppendText(e.KeyChar.ToString());
							AppendText("-");
							e.Handled = true;
							_delimitNumber++;
						}
					}
					
					if (_digitPos == pos && _delimitNumber == 0)
					{
						if (e.KeyChar != '-')
						{
							AppendText(e.KeyChar.ToString());
							AppendText("-");
							e.Handled = true;
							_delimitNumber++;
						}
					}

					if (_digitPos > 4)
						e.Handled = true;
				}
				else
				{
					e.Handled = false;
					if ((len - indx) == 1)
					{
						_delimitNumber--;
						_digitPos = indx > -1 ? len - indx : _digitPos - 1;
					}
					else
						_digitPos = indx > -1 ? len - indx - 1 : len - 1;
				}
			}
			else
			{
				e.Handled = true;
				_errorProvider.SetError(this, GetStr("ONLYDIGITANDDASH"));
			}
		}
		private void MaskIpAddr(KeyPressEventArgs e)
		{
			int len = Text.Length;
			int indx = Text.LastIndexOf(".");

			// if test is highlighted reset vars
			if (SelectedText == Text)
			{
				indx = -1;
				_digitPos = 0;
				_delimitNumber = 0;
			}

			if (char.IsDigit(e.KeyChar) || e.KeyChar == '.' || e.KeyChar == 8)
			{
				// only digit, Backspace and dot are accepted
				string tmp = Text;
				_errorProvider.SetError(this, string.Empty);
				if (e.KeyChar != 8)
				{
					if (TextLength == MaxLength)
					{
						e.Handled = true;
						return;
					}

					if (e.KeyChar != '.')
					{
						_digitPos = indx > 0 ? len - indx : _digitPos + 1;

						if (_digitPos == 3)
						{
							string tmp2 = Text.Substring(indx + 1) + e.KeyChar;

							if (int.Parse(tmp2) > 255) // check validation
								_errorProvider.SetError(this, string.Format("{0} 255", GetStr("NUMBERISSMALLERTHAN")));
							else
							{
								if (_delimitNumber < 3)
								{
									AppendText(e.KeyChar.ToString());
									AppendText(".");
									_delimitNumber++;
									e.Handled = true;
								}
							}
						}
						if (_digitPos == 4)
						{
							if (_delimitNumber < 3)
							{
								AppendText(".");
								_delimitNumber++;
							}
							else
								e.Handled = true;
						}
					}
					else
						_delimitNumber++;
				}
				else
				{
					e.Handled = false;
					if ((len - indx) == 1)
					{
						_delimitNumber--;

						_digitPos = indx > -1 
							? len - indx 
							: _digitPos - 1;
					}
					else
						_digitPos = indx > -1 ? len - indx - 1 : len - 1;
				}
			}
			else
			{
				e.Handled = true;
				_errorProvider.SetError(this, GetStr("ONLYDIGITANDDOT"));
			}
		}

		private bool CheckDayOfMonth(int mon, int day)
		{
			bool ret = true;
			if (day == 0) ret = false;
			switch (mon)
			{
				case 1:
				case 3:
				case 5:
				case 7:
				case 8:
				case 10:
				case 12:
					if (day > 31)
						ret = false;
					break;
				case 2:
					DateTime moment = DateTime.Now;
					int year = moment.Year;
					int d = ((year % 4 == 0) && ((!(year % 100 == 0)) || (year % 400 == 0))) ? 29 : 28;
					if (day > d)
						ret = false;
					break;
				case 4:
				case 6:
				case 9:
				case 11:
					if (day > 30)
						ret = false;
					break;
				default:
					ret = false;
					break;
			}
			return ret;
		}

        #region Dispose overide
        protected override void Dispose(bool disposing)
		{
			if (disposing)
				if (_components != null)
					_components.Dispose();

			base.Dispose(disposing);
		}
        #endregion
    }
}
