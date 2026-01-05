#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
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
        // Stores the timestamp from the most recent market data event
        // This gives us the "current time" according to the data feed
        private DateTime _lastMarketTime;
        
        // Flag to track whether we've received any market data yet
        // Prevents drawing before we have a valid timestamp
        private bool _hasMarketTime;
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Displays seconds remaining in the current time-based bar (Second/Minute) in the bottom-right corner.";
                Name = "MyCountdownTimer";
                
                // Update on every tick for smooth countdown
                Calculate = Calculate.OnEachTick;
                
                // Draw on the price chart (not in separate panel)
                IsOverlay = true;
                
                // Don't clutter the data box with this indicator
                DisplayInDataBox = false;
                
                // Keep updating even when chart isn't active window
                IsSuspendedWhileInactive = false;
            }
        }
        
        /// <summary>
        /// Calculates how many seconds a single bar lasts based on the chart's timeframe.
        /// For example: 5-minute chart = 300 seconds, 30-second chart = 30 seconds.
        /// </summary>
        /// <returns>Seconds per bar, or 0 if bar type doesn't support time-based duration</returns>
        private int GetSecondsInBar()
        {
            // Minute-based bars: multiply minutes by 60 to get seconds
            if (BarsPeriod.BarsPeriodType == BarsPeriodType.Minute)
                return BarsPeriod.Value * 60;
            
            // Second-based bars: value is already in seconds
            if (BarsPeriod.BarsPeriodType == BarsPeriodType.Second)
                return BarsPeriod.Value;
            
            // Tick, Volume, Range, or other non-time-based bars don't have duration
            return 0;
        }
        
        /// <summary>
        /// Clamps the remaining seconds to a valid range (0 to bar duration).
        /// Prevents showing negative numbers or values larger than the bar duration.
        /// Uses ceiling to round up partial seconds.
        /// </summary>
        /// <param name="remainingSeconds">Raw calculated seconds remaining</param>
        /// <param name="barSeconds">Total seconds in this bar</param>
        /// <returns>Clamped integer seconds, always between 0 and barSeconds</returns>
        private static int ClampRemaining(double remainingSeconds, int barSeconds)
        {
            // If negative (bar already closed), show 0
            if (remainingSeconds < 0) 
                remainingSeconds = 0;
            
            // If larger than bar duration (timing glitch), cap at bar duration
            if (remainingSeconds > barSeconds) 
                remainingSeconds = barSeconds;
            
            // Round up to nearest second (59.1 seconds shows as 60)
            return (int)Math.Ceiling(remainingSeconds);
        }
        
        /// <summary>
        /// Called every time market data arrives (ticks, bid/ask updates, trades).
        /// This is our "clock" - we use the exchange timestamp instead of system time
        /// to avoid timezone issues.
        /// </summary>
        /// <param name="e">Market data event containing timestamp and data type</param>
        protected override void OnMarketData(MarketDataEventArgs e)
        {
            // Only use trade, bid, or ask events as clock source
            // Ignore other market data types (like settlement prices)
            if (e.MarketDataType != MarketDataType.Last &&
                e.MarketDataType != MarketDataType.Bid &&
                e.MarketDataType != MarketDataType.Ask)
                return;
            
            // Store the timestamp from the market data feed
            // This is the exchange's time, not your computer's time
            _lastMarketTime = e.Time;
            _hasMarketTime = true;
            
            // Update the countdown display immediately
            DrawCountdown();
        }
        
        /// <summary>
        /// Called when a bar closes or updates.
        /// We also update the display here to catch bar transitions.
        /// </summary>
        protected override void OnBarUpdate()
        {
            // Update display when bars change
            DrawCountdown();
        }
        
        /// <summary>
        /// Core logic: calculates time remaining and draws it on the chart.
        /// </summary>
        private void DrawCountdown()
        {
            // Don't draw until we have a valid market timestamp
            if (!_hasMarketTime)
                return;
            
            // Need at least one bar to calculate
            if (CurrentBar < 1)
                return;
            
            // Get how long each bar lasts
            int barSeconds = GetSecondsInBar();
            
            // If this bar type doesn't support time duration, bail out
            if (barSeconds <= 0)
                return;
            
            // Use the market data timestamp as "current time"
            DateTime now = _lastMarketTime;
            
            // CRITICAL FIX: NinjaTrader sometimes reports Time[0] as the timestamp
            // of the NEXT bar that will form, not the current bar.
            // If Time[0] is in the future compared to the tick timestamp,
            // use Time[1] (previous bar) as the actual current bar start.
            DateTime barStart = Time[0];
            if (barStart > now && CurrentBar >= 1)
                barStart = Time[1];
            
            // Calculate when this bar will close
            DateTime barEnd = barStart.AddSeconds(barSeconds);
            
            // How many seconds until bar closes?
            double remainingSeconds = (barEnd - now).TotalSeconds;
            
            // Clamp to valid range (0 to bar duration)
            int remaining = ClampRemaining(remainingSeconds, barSeconds);
            
            // Draw the countdown in bottom-right corner
            // TextFixed means it stays in a fixed position on the chart
            Draw.TextFixed(
                this,
                "countdown_fixed",                      // Unique tag for this drawing
                $"Remaining: {remaining}s",             // Text to display
                TextPosition.BottomRight,               // Where to position it
                Brushes.DeepSkyBlue,                    // Text color
                new SimpleFont("Consolas", 16),         // Font (monospace for clean look)
                Brushes.Transparent,                    // Background color (transparent = no box)
                Brushes.Transparent,                    // Border color (transparent = no border)
                0);                                     // Opacity (0 = background fully transparent)
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

namespace NinjaTrader.NinjaScript.Strategies
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
