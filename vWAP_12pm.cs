// MagicKingdom - trading strategy for fading vWAP deviations and returns to vWAP. 
// Added in code for trailing stops, buying and selling within 1%, and trying to add in tick counting

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Principal;
using TradingPlatform.BusinessLayer;
using TradingPlatform.BusinessLayer.Modules;


namespace VWAP_StDeviation
{
    public class VWAP_StDeviation : Strategy, ICurrentAccount, ICurrentSymbol
    {
        [InputParameter("Symbol", 0)]
        public Symbol CurrentSymbol { get; set; }

        /// <summary>
        /// Account to place orders
        /// </summary>
        [InputParameter("Account", 1)]
        public Account CurrentAccount { get; set; }

        /// <summary>
        /// Period to load history
        /// </summary>
        [InputParameter("Period", 5)]
        private Period period = Period.MIN5;

        /// <summary>
        /// Period for VWAP indicator
        /// </summary>
        [InputParameter("VWAP Period", 2, minimum: 1, maximum: 100, increment: 1, decimalPlaces: 0)]
        public int VWAPPeriod = 14;

        /// <summary>
        /// Multiplier for VWAP
        /// </summary>
        [InputParameter("stdDevMultiplier", 3, minimum: 0.1, maximum: 100, increment: 0.1, decimalPlaces: 1)]
        public double stdDevMultiplier = 1.6;

        /// <summary>
        /// Quantity to open order
        /// </summary>
        [InputParameter("Quantity", 4, 0.1, 99999, 0.1, 2)]
        public double Quantity = 1.0;

        public override string[] MonitoringConnectionsIds => new string[] { this.CurrentSymbol?.ConnectionId, this.CurrentAccount?.ConnectionId };

        private Indicator vwap;

        private HistoricalData hdm;

        private int longPositionsCount;
        private int shortPositionsCount;
        private string orderTypeId;

        private bool waitOpenPosition;
        private bool waitClosePositions;

        public VWAP_StDeviation()
            : base()
        {
            this.Name = "VWAP_StDeviation";
            this.Description = "Raw strategy without any additional functional";
        }

        protected override void OnRun()
        {
            // Restore account object from acive connection
            if (this.CurrentAccount != null && this.CurrentAccount.State == BusinessObjectState.Fake)
                this.CurrentAccount = Core.Instance.GetAccount(this.CurrentAccount.CreateInfo());

            // Restore symbol object from acive connection
            if (this.CurrentSymbol != null && this.CurrentSymbol.State == BusinessObjectState.Fake)
                this.CurrentSymbol = Core.Instance.GetSymbol(this.CurrentSymbol.CreateInfo());

            if (this.CurrentSymbol == null || this.CurrentAccount == null || this.CurrentSymbol.ConnectionId != this.CurrentAccount.ConnectionId)
            {
                this.Log("Incorrect input parameters... Symbol or Account are not specified or they have different connectionID.", StrategyLoggingLevel.Error);
                return;
            }

            this.orderTypeId = Core.OrderTypes.FirstOrDefault(x => x.ConnectionId == this.CurrentSymbol.ConnectionId && x.Behavior == OrderTypeBehavior.Market).Id;

            if (string.IsNullOrEmpty(this.orderTypeId))
            {
                this.Log("Connection of selected symbol has not support market orders", StrategyLoggingLevel.Error);
                return;
            }

            // Create the VWAP indicator
            this.vwap = Indicator.VWAP(vwapPeriod);

            // Get the history data for the symbol
            this.hdm = this.CurrentSymbol.GetHistory(this.Period, this.CurrentSymbol.HistoryType, Core.TimeUtils.DateTimeUtcNow.AddDays(-100));

            // Add the VWAP indicator to the history data
            this.hdm.AddIndicator(this.vwap);
            var stdDev = this.vwap.StandardDeviation(this.VWAPPeriod);

            // Subscribe to the position added and removed events
            Core.PositionAdded += this.Core_PositionAdded;
            Core.PositionRemoved += this.Core_PositionRemoved;

            Core.OrdersHistoryAdded += this.Core_OrdersHistoryAdded;

            // Subscribe to the history data updated event
            this.hdm.HistoryItemUpdated += this.Hdm_HistoryItemUpdated;

            while (this.IsRunning)
            {
                var vwapPrice = this.vwap.LastValue;
                var buyPrice = vwapPrice - stdDev * this.stdDevMultiplier;
                var sellPrice = vwapPrice + stdDev * this.stdDevMultiplier;

                if (this
        
    private void Core_PositionAdded(Position position)
                    {
                        if (position.SymbolId == this.CurrentSymbol.Id && position.AccountId == this.CurrentAccount.Id)
                        {
                            if (position.Type == PositionType.Long)
                            {
                                // Check if the current price is at or beyond the sell level
                                if (position.Symbol.Ask >= sellLevel)
                                {
                                    // Close the position
                                    position.Close();
                                }
                            }
                            else if (position.Type == PositionType.Short)
                            {
                                // Check if the current price is at or beyond the buy level
                                if (position.Symbol.Bid <= buyLevel)
                                {
                                    // Close the position
                                    position.Close();
                                    if (this
                                    double buyThreshold = buyPrice * 0.01; // 1% threshold for buy level
                                    double sellThreshold = sellPrice * 0.01; // 1% threshold for sell level

                                    if (this.CurrentSymbol.LastPrice > buyPrice - buyThreshold && this.CurrentSymbol.LastPrice < buyPrice + buyThreshold)
                                    {
                                        // Place a buy order 
                                    }
                                    else if (this.CurrentSymbol.LastPrice > sellPrice - sellThreshold && this.CurrentSymbol.LastPrice < sellPrice + sellThreshold)
                                    {
                                        // Place a sell order

                                        // Initialize trailing stop loss variable
                                        var trailingStopLoss = 0.0;

                                        // Create event handler for history item updated event
                                        hdm.HistoryItemUpdated += (sender, args) =>
                                        {
                                            // Get current position
                                            var currentPosition = CurrentAccount.GetPosition(CurrentSymbol);

                                            // Check if current position is long and current price is greater than previous price
                                            if (currentPosition != null && currentPosition.IsLong && args.Current.Close > args.Previous.Close)
                                            {
                                                // Update trailing stop loss value
                                                trailingStopLoss = args.Current.Close - (args.Current.Close * 0.01); // 1% trailing stop loss
                                            }
                                            // Check if current position is short and current price is less than previous price
                                            else if (currentPosition != null && currentPosition.IsShort && args.Current.Close < args.Previous.Close)
                                            {
                                                // Update trailing stop loss value
                                                trailingStopLoss = args.Current.Close + (args.Current.Close * 0.01); // 1% trailing stop loss
                                            }

                                            // Check if current price has reached trailing stop loss value
                                            if (currentPosition != null && args.Current
                                
        // Subscribe to the history item updated event
        this.hdm.HistoryItemUpdated += this.Vwap_IndicatorUpdated;
                                        }
                                    
        private void Hdm_HistoryItemUpdated(object sender, HistoryItemEventArgs e)
                                        {
                                            if (this.hdm.Items.Count < this.VWAPPeriod)
                                                return;

                                            // Get the current price
                                            double currentPrice = e.Item.ClosePrice;

                                            // Get the value of the VWAP indicator
                                            double VWAPValue = this.indicatorVWAP.GetValue(this.hdm.Items.Count - 1);

                                            // Calculate the trailing stop value
                                            double trailingStop = e.Item.ClosePrice - this.stdDevMultiplier * VWAPValue;

                                            // Check if the current price crosses above the trailing stop
                                            if (currentPrice > trailingStop && this.shortPositionsCount > 0)
                                            {
                                                // Close all short positions

                                                // Subscribe to the position added event
                                                Core.PositionAdded += this.Core_PositionAdded;

                                                private void Core_PositionAdded(Position position)
                                                {
                                                    // check if the current position is a long position
                                                    if (position.TradeType == TradeType.Buy)
                                                    {
                                                        // set the initial trailing stop loss value
                                                        double trailingStopLoss = position.OpenPrice * (1 - 0.01); // 1% trailing stop loss

                                                        // check if the current price is greater than the previous price
                                                        if (position.CurrentPrice > trailingStopLoss)
                                                        {
                                                            // update the trailing stop loss value
                                                            trailingStopLoss = position.CurrentPrice * (1 - 0.01); // 1% trailing stop loss

                                                            // set the stop loss order
                                                            position.StopLoss = trailingStopLoss;
                                                        }

// Don't forget to unsubscribe from the event when the strategy is stopped
protected override void OnStop()
        {
            Core.PositionAdded -= this.Core_PositionAdded;

            [
            Core.Tick += this.OnTick;
        }

        private void OnTick(Tick tick)
        {
            var vwapPrice = this.vwap.LastValue;
            var buyPrice = vwapPrice - stdDev * this.stdDevMultiplier;
            var sellPrice = vwapPrice + stdDev * this.stdDevMultiplier;

            // Check if the current price is near the buy level
            if (tick.Price >= buyPrice && tick.Price <= buyPrice + 0.5)
            {
                //     Place a buy order 
            }
            // Check if the current price is near the sell level
            if (tick.Price <= sellPrice && tick.Price >= sellPrice - 0.5)
            {
                //      Place a buy order 
            }
        }

        protected override void OnStop()
        {
            Core.PositionAdded -= this.Core_PositionAdded;
            Core.PositionRemoved -= this.Core_PositionRemoved;

            Core.OrdersHistoryAdded -= this.Core_OrdersHistoryAdded;

            if (this.hdm != null)
            {
                this.hdm.HistoryItemUpdated -= this.Vwap_IndicatorUpdated;
                this.hdm.Dispose();
            }

            base.OnStop();
        }

        protected override List<StrategyMetric> OnGetMetrics()
        {
            var result = base.OnGetMetrics();

            // An example of adding custom strategy metrics:
            result.Add("Total long positions", this.longPositionsCount.ToString());
            result.Add("Total short positions", this.shortPositionsCount.ToString());

            return result;
        }

        private void Core_PositionAdded(Position obj)
        {
            var positions = Core.Instance.Positions.Where(x => x.Symbol == this.CurrentSymbol && x.Account == this.CurrentAccount).ToArray();
            this.longPositionsCount = positions.Count(x => x.Side == Side.Buy);
            this.shortPositionsCount = positions.Count(x => x.Side == Side.Sell);

            double currentPositionsQty = positions.Sum(x => x.Side == Side.Buy ? x.Quantity : -x.Quantity);

            if (Math.Abs(currentPositionsQty) == this.Quantity)
                this.waitOpenPosition = false;
        }

        private void Core_PositionRemoved(Position obj)
        {
            var positions = Core.Instance.Positions.Where(x => x.Symbol == this.CurrentSymbol && x.Account == this.CurrentAccount).ToArray();
            this.longPositionsCount = positions.Count(x => x.Side == Side.Buy);
            this.shortPositionsCount = positions.Count(x => x.Side == Side.Sell);

            if (!positions.Any())
                this.waitClosePositions = false;
        }

        private void Core_OrdersHistoryAdded(OrderHistory obj)
        {
            if (obj.Symbol == this.CurrentSymbol)
                return;

            if (obj.Account == this.CurrentAccount)
                return;

            if (obj.Status == OrderStatus.Refused)
                this.ProcessTradingRefuse();
        }

        private void Hdm_HistoryItemUpdated(object sender, HistoryEventArgs e) => this.OnUpdate();

        private void OnUpdate()
        {
            var positions = Core.Instance.Positions.Where(x => x.Symbol == this.CurrentSymbol && x.Account == this.CurrentAccount).ToArray();

            if (this.waitOpenPosition)
                return;

            if (this.waitClosePositions)
                return;

            if (positions.Any())
            {

                //Closing positions
                if (this.indicatorVWAP.GetValue(1) < this.indicatorVWAP.GetValue(1) || this.indicatorVWAP.GetValue(1) > this.indicatorVWAP.GetValue(1))
                {
                    this.waitClosePositions = true;
                    this.Log($"Start close positions ({positions.Length})");

                    foreach (var item in positions)
                    {
                        var result = item.Close();

                        if (result.Status == TradingOperationResultStatus.Failure)
                            this.ProcessTradingRefuse();
                        else
                            this.Log($"Position was close: {result.Status}", StrategyLoggingLevel.Trading);
                    }
                }
            }
            else
            {
                // Opening new positions
                if (this.indicatorVWAP.GetValue(2) < this.indicatorVWAP.GetValue(2) && this.indicatorVWAP.GetValue(1) > this.indicatorVWAP.GetValue(1))
                {
                    this.waitOpenPosition = true;
                    this.Log("Start open buy position");
                    var result = Core.Instance.PlaceOrder(new PlaceOrderRequestParameters()
                    {
                        Account = this.CurrentAccount,
                        Symbol = this.CurrentSymbol,

                        OrderTypeId = this.orderTypeId,
                        Quantity = this.Quantity,
                        Side = Side.Buy,
                    });

                    if (result.Status == TradingOperationResultStatus.Failure)
                        this.ProcessTradingRefuse();
                    else
                        this.Log($"Position open: {result.Status}", StrategyLoggingLevel.Trading);
                }
                else if (this.indicatorVWAP.GetValue(2) > this.indicatorVWAP.GetValue(2) && this.indicatorVWAP.GetValue(1) < this.indicatorVWAP.GetValue(1))
                {
                    this.waitOpenPosition = true;
                    this.Log("Start open sell position");
                    var result = Core.Instance.PlaceOrder(new PlaceOrderRequestParameters()
                    {
                        Account = this.CurrentAccount,
                        Symbol = this.CurrentSymbol,

                        OrderTypeId = this.orderTypeId,
                        Quantity = this.Quantity,
                        Side = Side.Sell,
                    });

                    if (result.Status == TradingOperationResultStatus.Failure)
                        this.ProcessTradingRefuse();
                    else
                        this.Log($"Position open: {result.Status}", StrategyLoggingLevel.Trading);
                }
            }
        }

        private void ProcessTradingRefuse()
        {
            this.Log("Strategy have received refuse for trading action. It should be stopped", StrategyLoggingLevel.Error);
            this.Stop();
        }
    }
}



namespace OrderPlacingStrategy1_VWAP_StDeviation
{
    /// <summary>
    /// Information about API you can find here: http://api.quantower.com
    /// Code samples: https://github.com/Quantower/Examples 
    /// </summary>
    public class OrderPlacingStrategy1_VWAP_StDeviation : OrderPlacingStrategy
    {
        public OrderPlacingStrategy1_VWAP_StDeviation()
            : base()
        {
            // Defines strategy's name and description.
            this.Name = "OrderPlacingStrategy1_VWAP_StDeviation";
            this.Description = "My OrderPlacingStrategy's annotation";
        }

        protected override void OnPlaceOrder(PlaceOrderRequestParameters placeOrderRequest)
        {
            throw new NotImplementedException();
        }

        protected override void OnCancel()
        {
            throw new NotImplementedException();
        }
    }
}
