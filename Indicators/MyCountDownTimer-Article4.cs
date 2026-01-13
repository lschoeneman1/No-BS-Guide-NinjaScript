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
    /// <summary>
    /// Countdown timer indicator that displays the time remaining until the current bar closes.
    /// Supports multiple display formats, customizable colors, position, and threshold-based warnings.
    /// Works with minute-based and second-based bar types.
    /// </summary>
    public class MyCountdownTimer : Indicator
    {
        #region Variables
        
        // Stores the timestamp from the most recent market data event
        // This gives us the "current time" according to the data feed
        private DateTime _lastMarketTime;
        
        // Flag to track whether we've received any market data yet
        // Prevents drawing before we have a valid timestamp
        private bool _hasMarketTime;
        
        #endregion
        
        #region OnStateChange
        
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Countdown timer with customizable display";
                Name = "MyCountdownTimer";
                
                // Update on every tick for smooth countdown
                Calculate = Calculate.OnEachTick;
                
                // Draw on the price chart (not in separate panel)
                IsOverlay = true;
                
                // Don't clutter the data box with this indicator
                DisplayInDataBox = false;
                
                // Don't paint price markers
                PaintPriceMarkers = false;
                
                // Keep updating even when chart isn't active window
                IsSuspendedWhileInactive = false;
                
                // Display properties - control where and how the timer appears
                DisplayPosition = TextPosition.BottomRight;
                FontSize = 16;
                
                // Color properties - visual customization
                TextColor = Brushes.DeepSkyBlue;
                BackgroundColor = Brushes.Transparent;
                BorderColor = Brushes.DeepSkyBlue;
                BorderOpacity = 0;
                
                // Format properties - how the countdown is displayed
                DisplayMode = 0; // 0=Seconds, 1=Min:Sec, 2=Percent, 3=Combo
                
                // Warning properties - change colors based on time remaining
                EnableColorWarnings = false;
                GreenThreshold = 60;   // Turn green when <= 60 seconds remain
                YellowThreshold = 30;  // Turn yellow when <= 30 seconds remain
                RedThreshold = 10;     // Turn red when <= 10 seconds remain
            }
        }
        
        #endregion
        
        #region Helper Methods
        
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
        /// Uses ceiling to round up partial seconds (e.g., 59.1 shows as 60).
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
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Called every time market data arrives (ticks, bid/ask updates, trades).
        /// This is our "clock" - we use the exchange timestamp instead of system time
        /// to avoid timezone issues and ensure accuracy.
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
        
        #endregion
        
        #region Drawing Logic
        
        /// <summary>
        /// Core logic: calculates time remaining in the current bar and draws it on the chart.
        /// Handles different display modes, color warnings, and customizable positioning.
        /// </summary>
        private void DrawCountdown()
        {
            // Don't draw until we have a valid market timestamp
            if (!_hasMarketTime)
                return;
            
            // Need at least one bar to calculate
            if (CurrentBar < 1)
                return;
            
            // Get how long each bar lasts (in seconds)
            int barSeconds = GetSecondsInBar();
            
            // If this bar type doesn't support time duration, bail out
            // (e.g., Renko, Range, Tick, Volume bars)
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
            
            // Build display text based on selected mode
            string displayText = "";
            switch (DisplayMode)
            {
                case 0: // Seconds - simple "45s" format
                    displayText = $"{remaining}s";
                    break;
                    
                case 1: // Minutes:Seconds - formatted as "1:45"
                    int minutes = remaining / 60;
                    int seconds = remaining % 60;
                    displayText = $"{minutes}:{seconds:D2}";
                    break;
                    
                case 2: // Percentage - shows "75.0%" of bar remaining
                    int totalSeconds = barSeconds;
                    double percentage = ((double)remaining / totalSeconds) * 100;
                    displayText = $"{percentage:F1}%";
                    break;
                    
                case 3: // Combo - shows both formats "1:45 (105s)"
                    int mins = remaining / 60;
                    int secs = remaining % 60;
                    displayText = $"{mins}:{secs:D2} ({remaining}s)";
                    break;
            }
            
            // Determine text color based on warning thresholds
            Brush currentColor = TextColor;
            if (EnableColorWarnings)
            {
                // Check thresholds from most urgent to least urgent
                if (remaining <= RedThreshold)
                    currentColor = Brushes.Red;
                else if (remaining <= YellowThreshold)
                    currentColor = Brushes.Yellow;
                else if (remaining <= GreenThreshold)
                    currentColor = Brushes.Green;
                // If above all thresholds, use default TextColor
            }
            
            // Draw the countdown text at the specified position
            // TextFixed means it stays in a fixed position on the chart regardless of zoom/scroll
            Draw.TextFixed(
                this,
                "countdown_fixed",              // Unique tag for this drawing object
                displayText,                    // The formatted countdown text
                DisplayPosition,                // Where to position it (e.g., BottomRight)
                currentColor,                   // Text color (may change based on warnings)
                new SimpleFont("Arial", FontSize), // Font and size
                BackgroundColor,                // Background color behind text
                BorderColor,                    // Border color around text box
                BorderOpacity);                 // Border opacity (0-100)
        }
        
        #endregion
        
        #region Properties
        
        // Display properties
        [NinjaScriptProperty]
        [Display(Name = "Position", Description = "Where to display the timer on chart", GroupName = "Display", Order = 1)]
        public TextPosition DisplayPosition { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Font Size", Description = "Size of the timer text", GroupName = "Display", Order = 2)]
        [Range(8, 72)]
        public int FontSize { get; set; }

        // Color properties
        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Text Color", Description = "Color of the timer text", GroupName = "Colors", Order = 1)]
        public Brush TextColor { get; set; }

        [Browsable(false)]
        public string TextColorSerializable
        {
            get { return Serialize.BrushToString(TextColor); }
            set { TextColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Background Color", Description = "Background color behind text", GroupName = "Colors", Order = 2)]
        public Brush BackgroundColor { get; set; }

        [Browsable(false)]
        public string BackgroundColorSerializable
        {
            get { return Serialize.BrushToString(BackgroundColor); }
            set { BackgroundColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [XmlIgnore]
        [Display(Name = "Border Color", Description = "Border color around text box", GroupName = "Colors", Order = 3)]
        public Brush BorderColor { get; set; }

        [Browsable(false)]
        public string BorderColorSerializable
        {
            get { return Serialize.BrushToString(BorderColor); }
            set { BorderColor = Serialize.StringToBrush(value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Border Opacity", Description = "Border opacity (0-100)", GroupName = "Colors", Order = 4)]
        [Range(0, 100)]
        public int BorderOpacity { get; set; }

        // Format properties
        [NinjaScriptProperty]
        [Display(Name = "Display Mode", Description = "0=Seconds, 1=Min:Sec, 2=Percent, 3=Combo", GroupName = "Format", Order = 1)]
        [Range(0, 3)]
        public int DisplayMode { get; set; }

        // Warning properties
        [NinjaScriptProperty]
        [Display(Name = "Enable Color Warnings", Description = "Change color based on thresholds", GroupName = "Warnings", Order = 1)]
        public bool EnableColorWarnings { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Green Threshold", Description = "Seconds remaining for green warning", GroupName = "Warnings", Order = 2)]
        [Range(1, 3600)]
        public int GreenThreshold { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Yellow Threshold", Description = "Seconds remaining for yellow warning", GroupName = "Warnings", Order = 3)]
        [Range(1, 3600)]
        public int YellowThreshold { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Red Threshold", Description = "Seconds remaining for red warning", GroupName = "Warnings", Order = 4)]
        [Range(1, 3600)]
        public int RedThreshold { get; set; }
        
        #endregion
    }
}

