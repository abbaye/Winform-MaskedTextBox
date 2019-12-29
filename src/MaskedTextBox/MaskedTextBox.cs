using System;
using System.ComponentModel;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MaskedTextBox
{
	public class MaskedTextBox : TextBox
	{
		private static readonly ResourceManager rscMgr =
			new ResourceManager("MaskedTextBox.MaskEditRes", typeof(MaskedTextBox).Assembly);

		private Mask m_mask;
		private int digitPos;
		private int DelimitNumber;
		private string fDateFormat;
		private ErrorProvider errorProvider1;
		private Container components;
		private int fMinimalYear;
		private int fMaxYear;

		public Mask Masked
		{
			get => m_mask;
			set
			{
				m_mask = value;
				Text = string.Empty;
				switch (m_mask)
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
			get => "dd/mm/yyyy" == fDateFormat 
				? FormatDate.ddmmyyyy 
				: FormatDate.mmddyyyy;
			set => fDateFormat = FormatDate.ddmmyyyy == value 
				? "dd/mm/yyyy" 
				: "mm/dd/yyyy";
		}

		public int MinYear
		{
			get => fMinimalYear;
			set
			{
				if (value >= DateTime.MinValue.Year)
					fMinimalYear = value;
			}
		}
		public int MaxYear
		{
			get => fMaxYear;
			set
			{
				if (value <= DateTime.MaxValue.Year)
					fMaxYear = value;
			}
		}
		public string Error => errorProvider1.GetError(this);

		public MaskedTextBox()
		{
			InitializeComponent();

			if (Masked != Mask.None)
				m_mask = Masked;

			fDateFormat = "dd/mm/yyyy";
			fMinimalYear = 1980;
			fMaxYear = 2050;
		}

		private static string GetStr(string key) => rscMgr.GetString(key);

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				if (components != null)
					components.Dispose();

			base.Dispose(disposing);
		}

		#region Component Designer generated code
		private void InitializeComponent()
		{
			components = null;
			digitPos = 0;
			DelimitNumber = 0;

			errorProvider1 = new ErrorProvider
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
			switch (m_mask)
			{
				case Mask.DateOnly:
					if ("dd/mm/yyyy" == fDateFormat)
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

			switch (m_mask)
			{
				case Mask.DateOnly:
					if (TextLength > 0)
					{
						regStr = new Regex(@"\d{2}/\d{2}/\d{4}");
						if (!regStr.IsMatch(sd.Text))
						{
							if ("dd/mm/yyyy" == fDateFormat)
								errorProvider1.SetError(this, GetStr("DATEFORMATddmmyyyy"));
							else
								errorProvider1.SetError(this, GetStr("DATEFORMAT"));
						}

						try
						{
							DateTime d = DateTime.Parse(Text);
							if (d.Year < fMinimalYear || fMaxYear < d.Year)
								errorProvider1.SetError(this, GetStr("YEARBETWEEN"));
						}
						catch (FormatException)
						{
							if ("dd/mm/yyyy" == fDateFormat)
								errorProvider1.SetError(this, GetStr("DATEFORMATddmmyyyy"));
							else
								errorProvider1.SetError(this, GetStr("DATEFORMAT"));
						}
					}
					else
						errorProvider1.SetError(this, string.Empty);
					break;
				case Mask.PhoneWithArea:
					regStr = new Regex(@"\d{3}-\d{3}-\d{4}");
					if (!regStr.IsMatch(sd.Text))
						errorProvider1.SetError(this, GetStr("PHONEFORMAT"));
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
									errorProvider1.SetError(this, GetStr("IP_FORMAT"));
									break;
								}
						}
					if (cnt < 3 || sd.Text[len - 1] == '.')
						errorProvider1.SetError(this, GetStr("IP_FORMAT"));
					break;
				case Mask.SSN:
					regStr = new Regex(@"\d{3}-\d{2}-\d{4}");
					if (!regStr.IsMatch(sd.Text))
						errorProvider1.SetError(this, GetStr("SSNFORMAT"));
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
				errorProvider1.SetError(this, string.Empty);
				e.Handled = false;
			}
			else
			{
				e.Handled = true;
				errorProvider1.SetError(this, GetStr("ONLYDIGIT"));
			}
		}
		private void MaskDecimal(KeyPressEventArgs e)
		{
			if (char.IsDigit(e.KeyChar) || e.KeyChar == '.' || e.KeyChar == 8)
			{
				e.Handled = false;
				errorProvider1.SetError(this, string.Empty);
			}
			else
			{
				e.Handled = true;
				errorProvider1.SetError(this, GetStr("ONLYDIGITANDDOT"));
			}
		}


		/// <summary>
		///  Support for min/max year in this format are not implemented
		/// </summary>
		private void MaskDate_ddmmyyyy(KeyPressEventArgs e)
		{
			errorProvider1.SetError(this, string.Empty);

			if (!char.IsDigit(e.KeyChar) && e.KeyChar != '/' && e.KeyChar != 8)
			{
				errorProvider1.SetError(this, GetStr("ONLYDIGITANDSLASH"));
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
						errorProvider1.SetError(this, GetStr("ONLYDIGIT"));
						noError = false;
					}
					else if (textValue.Length == 2 || textValue.Length == 5)
					{
						textValue = textValue.Insert(textValue.Length - 2, "0");
						indLastSlash = textValue.LastIndexOf("/");

						if (0 == int.Parse(textValue.Substring(indLastSlash - 2, 2)))
						{
							errorProvider1.SetError(this, "This is not a valid day of month");
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
								errorProvider1.SetError(this, string.Format("{0} 31", GetStr("NUMBERISSMALLERTHAN")));
								noError = false;
							}
							textValue += "/";
							break;
						case 4:
							Regex regStr = new Regex(@"\d{2}/\d{1}");
							if (!regStr.IsMatch(textValue))
							{
								errorProvider1.SetError(this, GetStr("DATEFORMATddmmyyyy"));
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
								errorProvider1.SetError(this, string.Format("{0} 12", GetStr("NUMBERISSMALLERTHAN")));
								noError = false;
							}
							else
								textValue += "/";
							break;
						case 10:
							int year = int.Parse(textValue.Substring(6, 4));
							if (year < fMinimalYear || year > fMaxYear)
							{
								errorProvider1.SetError(this, string.Format("{0}: {1:d}-{2:d}", GetStr("YEARBETWEEN"), fMinimalYear, fMaxYear));
								noError = false;
							}
							break;
					}
				}
				if (textValue.Length == 6 && !CheckDayOfMonth(int.Parse(textValue.Substring(3, 2)), int.Parse(textValue.Substring(0, 2))))
				{
					noError = false;
					errorProvider1.SetError(this, GetStr("DAYNOTVALID"));
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
				digitPos = 0;
				DelimitNumber = 0;
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
						if (indx > 0)
							digitPos = len - indx;
						else
							digitPos++;

						if (digitPos == 3 && DelimitNumber < 2)
							if (e.KeyChar != '/')
							{
								DelimitNumber++;
								AppendText("/");
							}

						errorProvider1.SetError(this, "");
						if (digitPos == 2 || (int.Parse(e.KeyChar.ToString()) > 1 && DelimitNumber == 0))
						{
							string tmp2 = indx == -1
								? e.KeyChar.ToString()
								: Text.Substring(indx + 1) + e.KeyChar.ToString();

							if (DelimitNumber < 2)
							{
								if (digitPos == 1) AppendText("0");
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
											DelimitNumber++;
											AppendText("/");
										}
										else
										{
											Text = str.Insert(2, "/");
											AppendText("");
										}
										DelimitNumber++;
									}
									else
									{
										AppendText("/");
										DelimitNumber++;
									}
									e.Handled = true;
								}
								else
								{
									if (DelimitNumber == 1)
									{
										int m = int.Parse(Text.Substring(0, indx));
										if (!CheckDayOfMonth(m, int.Parse(tmp2)))
											errorProvider1.SetError(this, string.Format("{0} 31", GetStr("NUMBERISSMALLERTHAN")));
										else
										{
											AppendText("/");
											DelimitNumber++;
											e.Handled = true;
										}
									}
								}
							}
						}
						else if (digitPos == 1 && int.Parse(e.KeyChar.ToString()) > 3 && DelimitNumber < 2)
						{
							if (digitPos == 1) AppendText("0");
							AppendText(e.KeyChar.ToString());
							AppendText("/");
							DelimitNumber++;
							e.Handled = true;
						}
						else
						{
							if (digitPos == 1 && DelimitNumber == 2 && e.KeyChar > '2')
								errorProvider1.SetError(this, "The year should start with 1 or 2");
						}
						if (digitPos > 4)
							e.Handled = true;
					}
					else
					{
						DelimitNumber++;

						string tmp3 = indx == -1
							? Text.Substring(indx + 1)
							: Text;

						if (digitPos == 1)
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
						DelimitNumber--;
						if (indx > -1)
							digitPos = 2;
						else
							digitPos--;
					}
					else
					{
						digitPos = indx > -1
							? len - indx - 1
							: len - 1;
					}
				}
			}
			else
			{
				e.Handled = true;
				errorProvider1.SetError(this, GetStr("ONLYDIGITANDSLASH"));
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
				digitPos = 0;
				DelimitNumber = 0;
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

					errorProvider1.SetError(this, string.Empty);
					if (e.KeyChar != '-')
					{
						if (indx > 0)
							digitPos = len - indx;
						else
							digitPos++;
					}
					if (indx > -1 && digitPos == pos2 && DelimitNumber == 1)
					{
						if (e.KeyChar != '-')
						{
							AppendText(e.KeyChar.ToString());
							AppendText("-");
							e.Handled = true;
							DelimitNumber++;
						}
					}
					if (digitPos == pos && DelimitNumber == 0)
					{
						if (e.KeyChar != '-')
						{
							AppendText(e.KeyChar.ToString());
							AppendText("-");
							e.Handled = true;
							DelimitNumber++;
						}
					}
					if (digitPos > 4)
						e.Handled = true;
				}
				else
				{
					e.Handled = false;
					if ((len - indx) == 1)
					{
						DelimitNumber--;
						if ((indx) > -1)
							digitPos = len - indx;
						else
							digitPos--;
					}
					else
					{
						digitPos = indx > -1
							? len - indx - 1
							: len - 1;
					}
				}
			}
			else
			{
				e.Handled = true;
				errorProvider1.SetError(this, GetStr("ONLYDIGITANDDASH"));
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
				digitPos = 0;
				DelimitNumber = 0;
			}

			if (char.IsDigit(e.KeyChar) || e.KeyChar == '.' || e.KeyChar == 8)
			{
				// only digit, Backspace and dot are accepted
				string tmp = Text;
				errorProvider1.SetError(this, string.Empty);
				if (e.KeyChar != 8)
				{
					if (TextLength == MaxLength)
					{
						e.Handled = true;
						return;
					}

					if (e.KeyChar != '.')
					{
						if (indx > 0)
							digitPos = len - indx;
						else
							digitPos++;

						if (digitPos == 3)
						{
							string tmp2 = Text.Substring(indx + 1) + e.KeyChar;
							if (int.Parse(tmp2) > 255) // check validation
								errorProvider1.SetError(this, string.Format("{0} 255", GetStr("NUMBERISSMALLERTHAN")));
							else
							{
								if (DelimitNumber < 3)
								{
									AppendText(e.KeyChar.ToString());
									AppendText(".");
									DelimitNumber++;
									e.Handled = true;
								}
							}
						}
						if (digitPos == 4)
						{
							if (DelimitNumber < 3)
							{
								AppendText(".");
								DelimitNumber++;
							}
							else
								e.Handled = true;
						}
					}
					else
						DelimitNumber++;
				}
				else
				{
					e.Handled = false;
					if ((len - indx) == 1)
					{
						DelimitNumber--;

						if (indx > -1)
							digitPos = len - indx;
						else
							digitPos--;
					}
					else
					{
						digitPos = indx > -1
							? len - indx - 1
							: len - 1;
					}
				}
			}
			else
			{
				e.Handled = true;
				errorProvider1.SetError(this, GetStr("ONLYDIGITANDDOT"));
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
	}
}
