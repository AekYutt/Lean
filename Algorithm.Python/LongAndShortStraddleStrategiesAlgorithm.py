# QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
# Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

import itertools
from AlgorithmImports import *

### <summary>
### This algorithm demonstrate how to use OptionStrategies helper class to batch send orders for common strategies.
### In this case, the algorithm tests the Straddle and Short Straddle strategies.
### </summary>
class LongAndShortStraddleStrategiesAlgorithm(QCAlgorithm):

    def Initialize(self):
        self.SetStartDate(2015, 12, 24)
        self.SetEndDate(2015, 12, 24)
        self.SetCash(1000000)

        option = self.AddOption("GOOG")
        self._option_symbol = option.Symbol

        option.SetFilter(-2, +2, 0, 180)

        self.SetBenchmark("GOOG")

    def OnData(self,slice):
        if not self.Portfolio.Invested:
            chain = slice.OptionChains.get(self._option_symbol)
            if chain is not None:
                contracts = sorted(sorted(chain, key=lambda x: abs(chain.Underlying.Price - x.Strike)),
                                   key=lambda x: x.Expiry, reverse=True)
                groupedContracts = [list(group) for _, group in itertools.groupby(contracts, lambda x: (x.Strike, x.Expiry))]
                groupedContracts = (group
                                    for group in groupedContracts
                                    if (any(contract.Right == OptionRight.Call for contract in group) and
                                        any(contract.Right == OptionRight.Put for contract in group)))
                contracts = next(groupedContracts, [])

                if len(contracts) == 0:
                    return

                contract = contracts[0]
                if contract is not None:
                    self._straddle = OptionStrategies.Straddle(self._option_symbol, contract.Strike, contract.Expiry)
                    self._short_straddle = OptionStrategies.ShortStraddle(self._option_symbol, contract.Strike, contract.Expiry)
                    self.Buy(self._straddle, 2)
        else:
            # Verify that the strategy was traded
            positionGroup = list(self.Portfolio.Positions.Groups)[0]

            buyingPowerModel = positionGroup.BuyingPowerModel
            if not isinstance(buyingPowerModel, OptionStrategyPositionGroupBuyingPowerModel):
                raise Exception("Expected position group buying power model type: OptionStrategyPositionGroupBuyingPowerModel. "
                                f"Actual: {type(positionGroup.BuyingPowerModel).__name__}")

            positions = list(positionGroup.Positions)
            if len(positions) != 2:
                raise Exception(f"Expected position group to have 2 positions. Actual: {len(positions)}")

            callPosition = next((position for position in positions if position.Symbol.ID.OptionRight == OptionRight.Call), None)
            if callPosition is None:
                raise Exception("Expected position group to have a call position")

            putPosition = next((position for position in positions if position.Symbol.ID.OptionRight == OptionRight.Put), None)
            if putPosition is None:
                raise Exception("Expected position group to have a put position")

            expectedCallPositionQuantity = 2
            expectedPutPositionQuantity = 2

            if callPosition.Quantity != expectedCallPositionQuantity:
                raise Exception(f"Expected call position quantity to be {expectedCallPositionQuantity}. Actual: {callPosition.Quantity}")

            if putPosition.Quantity != expectedPutPositionQuantity:
                raise Exception(f"Expected put position quantity to be {expectedPutPositionQuantity}. Actual: {putPosition.Quantity}")

            # Now we should be able to close the position using the inverse strategy (a short straddle)
            self.Buy(self._short_straddle, 2);

            # We can quit now, no more testing required
            self.Quit();

    def OnEndOfAlgorithm(self):
        if self.Portfolio.Invested:
            raise Exception("Expected no holdings at end of algorithm")

        orders_count = len(list(self.Transactions.GetOrders(lambda order: order.Status == OrderStatus.Filled)))
        if orders_count != 4:
            raise Exception("Expected 4 orders to have been submitted and filled, 2 for buying the straddle and 2 for the liquidation. "
                            f"Actual {orders_count}")

    def OnOrderEvent(self, orderEvent):
        self.Debug(str(orderEvent))
