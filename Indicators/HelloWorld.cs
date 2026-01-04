// ============================================================================
// The No-Bullshit Guide to NinjaScript
// Article #2
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
	public class HelloWorld : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "HelloWorld";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
 			Draw.Text(this, "hello", "Hello from NinjaScript!", 0, High[0]);		
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HelloWorld[] cacheHelloWorld;
		public HelloWorld HelloWorld()
		{
			return HelloWorld(Input);
		}

		public HelloWorld HelloWorld(ISeries<double> input)
		{
			if (cacheHelloWorld != null)
				for (int idx = 0; idx < cacheHelloWorld.Length; idx++)
					if (cacheHelloWorld[idx] != null &&  cacheHelloWorld[idx].EqualsInput(input))
						return cacheHelloWorld[idx];
			return CacheIndicator<HelloWorld>(new HelloWorld(), input, ref cacheHelloWorld);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HelloWorld HelloWorld()
		{
			return indicator.HelloWorld(Input);
		}

		public Indicators.HelloWorld HelloWorld(ISeries<double> input )
		{
			return indicator.HelloWorld(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HelloWorld HelloWorld()
		{
			return indicator.HelloWorld(Input);
		}

		public Indicators.HelloWorld HelloWorld(ISeries<double> input )
		{
			return indicator.HelloWorld(input);
		}
	}
}

#endregion
