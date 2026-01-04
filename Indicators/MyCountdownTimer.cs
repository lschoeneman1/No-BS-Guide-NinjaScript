// ============================================================================
// The No-Bullshit Guide to NinjaScript
// Article #3
// 
// Author: Larry Schoeneman
// Series: https://medium.com/itnext/the-no-bullshit-guide-to-ninjascript-part-2-e55b26717794
// Code Repo: https://github.com/lschoeneman1/No-BS-Guide-NinjaScript
//
// Usage:
// 1. Copy this file into NinjaScript Editor
// 2. Compile (F5)
// 3. Add to chart via Indicators → HelloWorld
// 4. See your text appear above bars
//
// License: MIT
// ============================================================================
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class MyCountdownTimer : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description = @"Shows time remaining in current bar";
    			Name = "Bar Countdown Timer";
    			Calculate = Calculate.OnEachTick;  // Update every tick (important!)
    			IsOverlay = true;                   // Draw on the main chart
    			DisplayInDataBox = false;           // Don't clutter the data box
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			// Safety check: make sure we actually have data
    		if (CurrentBar < 1)
			{
				return;
			}
			
			// STEP 1: Figure out how long this bar is supposed to last
			// If you're on a 5-minute chart, this gives you 5 minutes
			// If you're on a 15-minute chart, you get 15 minutes
			// You get the idea
			TimeSpan barDuration = TimeSpan.FromMinutes(BarsPeriod.Value);
		
			// STEP 2: When did this bar start?
			// Time[0] means "the time of the current bar"
			DateTime barStartTime = Time[0];
			
			// STEP 3: When will this bar end?
			// Start time + duration = end time (groundbreaking math, I know)
			DateTime barEndTime = barStartTime + barDuration;
			
			// STEP 4: How much time is left?
			// End time minus current time = time remaining
			TimeSpan timeRemaining = barEndTime - DateTime.Now;
			
			// STEP 5: Format it nicely and display it
			string displayText;
			
			if (timeRemaining.TotalSeconds > 0)
			{
				// Format as "2:34" (minutes:seconds)
				displayText = string.Format("{0}:{1:00}",
					(int)timeRemaining.TotalMinutes,  // Total minutes as whole number
					timeRemaining.Seconds);            // Just the seconds part
			}
			else
			{
				// Bar is closing or already closed
				displayText = "Closing...";
			}
			
			// STEP 6: Draw it on the chart
			// Put it above the current bar's high, in yellow
			Draw.Text(this, "countdown", displayText, 0, High[0] + 5 * TickSize, Brushes.Blue);
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MyCountdownTimer[] cacheMyCountdownTimer;
		public MyCountdownTimer MyCountdownTimer()
		{
			return MyCountdownTimer(Input);
		}

		public MyCountdownTimer MyCountdownTimer(ISeries<double> input)
		{
			if (cacheMyCountdownTimer != null)
				for (int idx = 0; idx < cacheMyCountdownTimer.Length; idx++)
					if (cacheMyCountdownTimer[idx] != null &&  cacheMyCountdownTimer[idx].EqualsInput(input))
						return cacheMyCountdownTimer[idx];
			return CacheIndicator<MyCountdownTimer>(new MyCountdownTimer(), input, ref cacheMyCountdownTimer);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MyCountdownTimer MyCountdownTimer()
		{
			return indicator.MyCountdownTimer(Input);
		}

		public Indicators.MyCountdownTimer MyCountdownTimer(ISeries<double> input )
		{
			return indicator.MyCountdownTimer(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies```````````
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MyCountdownTimer MyCountdownTimer()
		{
			return indicator.MyCountdownTimer(Input);
		}

		public Indicators.MyCountdownTimer MyCountdownTimer(ISeries<double> input )
		{
			return indicator.MyCountdownTimer(input);
		}
	}
}

#endregion
