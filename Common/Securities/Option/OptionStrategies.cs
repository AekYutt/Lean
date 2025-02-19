/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Securities.Option.StrategyMatcher;

namespace QuantConnect.Securities.Option
{
    /// <summary>
    /// Provides methods for creating popular <see cref="OptionStrategy"/> instances.
    /// These strategies can be directly bought and sold via:
    ///     QCAlgorithm.Buy(OptionStrategy strategy, int quantity)
    ///     QCAlgorithm.Sell(OptionStrategy strategy, int quantity)
    ///
    /// See also <see cref="OptionStrategyDefinitions"/>
    /// </summary>
    public static class OptionStrategies
    {
        /// <summary>
        /// Symbol properties database to use to get contract multipliers
        /// </summary>
        private static SymbolPropertiesDatabase _symbolPropertiesDatabase = SymbolPropertiesDatabase.FromDataFolder();

        /// <summary>
        /// Creates a Covered Call strategy that consists of selling one call contract and buying 1 lot of the underlying.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="strike">The strike price for the call option contract</param>
        /// <param name="expiration">The expiration date for the call option contract</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy CoveredCall(Symbol canonicalOption, decimal strike, DateTime expiration)
        {
            CheckCanonicalOptionSymbol(canonicalOption, "CoveredCall");
            CheckExpirationDate(expiration, "CoveredCall", nameof(expiration));

            var underlyingQuantity = (int)_symbolPropertiesDatabase.GetSymbolProperties(canonicalOption.ID.Market, canonicalOption,
                canonicalOption.SecurityType, "").ContractMultiplier;

            return new OptionStrategy
            {
                Name = OptionStrategyDefinitions.CoveredCall.Name,
                Underlying = canonicalOption.Underlying,
                CanonicalOption = canonicalOption,
                OptionLegs = new List<OptionStrategy.OptionLegData>
                {
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Call, Strike = strike, Quantity = -1, Expiration = expiration
                    }
                },
                UnderlyingLegs = new List<OptionStrategy.UnderlyingLegData>
                {
                    new OptionStrategy.UnderlyingLegData
                    {
                        Quantity = underlyingQuantity, Symbol = canonicalOption.Underlying
                    }
                }
            };
        }

        /// <summary>
        /// Creates a Protective Call strategy that consists of buying one call contract and selling 1 lot of the underlying.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="strike">The strike price for the call option contract</param>
        /// <param name="expiration">The expiration date for the call option contract</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy ProtectiveCall(Symbol canonicalOption, decimal strike, DateTime expiration)
        {
            // Since a protective call is an inverted covered call, we can just use the CoveredCall method and invert the legs
            return InvertStrategy(CoveredCall(canonicalOption, strike, expiration), OptionStrategyDefinitions.ProtectiveCall.Name);
        }

        /// <summary>
        /// Creates a Covered Put strategy that consists of selling 1 put contract and 1 lot of the underlying.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="strike">The strike price for the put option contract</param>
        /// <param name="expiration">The expiration date for the put option contract</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy CoveredPut(Symbol canonicalOption, decimal strike, DateTime expiration)
        {
            CheckCanonicalOptionSymbol(canonicalOption, "CoveredPut");
            CheckExpirationDate(expiration, "CoveredPut", nameof(expiration));

            var underlyingQuantity = -(int)_symbolPropertiesDatabase.GetSymbolProperties(canonicalOption.ID.Market, canonicalOption,
                canonicalOption.SecurityType, "").ContractMultiplier;

            return new OptionStrategy
            {
                Name = OptionStrategyDefinitions.CoveredPut.Name,
                Underlying = canonicalOption.Underlying,
                CanonicalOption = canonicalOption,
                OptionLegs = new List<OptionStrategy.OptionLegData>
                {
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Put, Strike = strike, Quantity = -1, Expiration = expiration
                    }
                },
                UnderlyingLegs = new List<OptionStrategy.UnderlyingLegData>
                {
                    new OptionStrategy.UnderlyingLegData
                    {
                        Quantity = underlyingQuantity, Symbol = canonicalOption.Underlying
                    }
                }
            };
        }

        /// <summary>
        /// Creates a Protective Put strategy that consists of buying 1 put contract and 1 lot of the underlying.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="strike">The strike price for the put option contract</param>
        /// <param name="expiration">The expiration date for the put option contract</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy ProtectivePut(Symbol canonicalOption, decimal strike, DateTime expiration)
        {
            // Since a protective put is an inverted covered put, we can just use the CoveredPut method and invert the legs
            return InvertStrategy(CoveredPut(canonicalOption, strike, expiration), OptionStrategyDefinitions.ProtectivePut.Name);
        }

        /// <summary>
        /// Creates a Naked Call strategy that consists of selling 1 call contract.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="strike">The strike price for the call option contract</param>
        /// <param name="expiration">The expiration date for the call option contract</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy NakedCall(Symbol canonicalOption, decimal strike, DateTime expiration)
        {
            CheckCanonicalOptionSymbol(canonicalOption, "NakedCall");
            CheckExpirationDate(expiration, "NakedCall", nameof(expiration));

            return new OptionStrategy
            {
                Name = OptionStrategyDefinitions.NakedCall.Name,
                Underlying = canonicalOption.Underlying,
                CanonicalOption = canonicalOption,
                OptionLegs = new List<OptionStrategy.OptionLegData>
                {
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Call, Strike = strike, Quantity = -1, Expiration = expiration
                    }
                }
            };
        }

        /// <summary>
        /// Creates a Naked Put strategy that consists of selling 1 put contract.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="strike">The strike price for the put option contract</param>
        /// <param name="expiration">The expiration date for the put option contract</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy NakedPut(Symbol canonicalOption, decimal strike, DateTime expiration)
        {
            CheckCanonicalOptionSymbol(canonicalOption, "NakedPut");
            CheckExpirationDate(expiration, "NakedPut", nameof(expiration));

            return new OptionStrategy
            {
                Name = OptionStrategyDefinitions.NakedPut.Name,
                Underlying = canonicalOption.Underlying,
                CanonicalOption = canonicalOption,
                OptionLegs = new List<OptionStrategy.OptionLegData>
                {
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Put, Strike = strike, Quantity = -1, Expiration = expiration
                    }
                }
            };
        }

        /// <summary>
        /// Method creates new Bear Call Spread strategy, that consists of two calls with the same expiration but different strikes.
        /// The strike price of the short call is below the strike of the long call. This is a credit spread.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="leg1Strike">The strike price of the short call</param>
        /// <param name="leg2Strike">The strike price of the long call</param>
        /// <param name="expiration">Option expiration date</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy BearCallSpread(
            Symbol canonicalOption,
            decimal leg1Strike,
            decimal leg2Strike,
            DateTime expiration
            )
        {
            CheckCanonicalOptionSymbol(canonicalOption, "BearCallSpread");
            CheckExpirationDate(expiration, "BearCallSpread", nameof(expiration));

            if (leg1Strike >= leg2Strike)
            {
                throw new ArgumentException("BearCallSpread: leg1Strike must be less than leg2Strike", "leg1Strike, leg2Strike");
            }

            return new OptionStrategy
            {
                Name = OptionStrategyDefinitions.BearCallSpread.Name,
                Underlying = canonicalOption.Underlying,
                CanonicalOption = canonicalOption,
                OptionLegs = new List<OptionStrategy.OptionLegData>
                {
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Call, Strike = leg1Strike, Quantity = -1, Expiration = expiration
                    },
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Call, Strike = leg2Strike, Quantity = 1, Expiration = expiration
                    }
                }
            };
        }

        /// <summary>
        /// Method creates new Bear Put Spread strategy, that consists of two puts with the same expiration but different strikes.
        /// The strike price of the short put is below the strike of the long put. This is a debit spread.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="leg1Strike">The strike price of the long put</param>
        /// <param name="leg2Strike">The strike price of the short put</param>
        /// <param name="expiration">Option expiration date</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy BearPutSpread(
            Symbol canonicalOption,
            decimal leg1Strike,
            decimal leg2Strike,
            DateTime expiration
            )
        {
            CheckCanonicalOptionSymbol(canonicalOption, "BearPutSpread");
            CheckExpirationDate(expiration, "BearPutSpread", nameof(expiration));

            if (leg1Strike <= leg2Strike)
            {
                throw new ArgumentException("BearPutSpread: leg1Strike must be greater than leg2Strike", "leg1Strike, leg2Strike");
            }

            return new OptionStrategy
            {
                Name = OptionStrategyDefinitions.BearPutSpread.Name,
                Underlying = canonicalOption.Underlying,
                CanonicalOption = canonicalOption,
                OptionLegs = new List<OptionStrategy.OptionLegData>
                {
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Put, Strike = leg1Strike, Quantity = 1,
                        Expiration = expiration
                    },
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Put, Strike = leg2Strike, Quantity = -1, Expiration = expiration
                    }
                }
            };
        }

        /// <summary>
        /// Method creates new Bull Call Spread strategy, that consists of two calls with the same expiration but different strikes.
        /// The strike price of the short call is higher than the strike of the long call. This is a debit spread.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="leg1Strike">The strike price of the long call</param>
        /// <param name="leg2Strike">The strike price of the short call</param>
        /// <param name="expiration">Option expiration date</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy BullCallSpread(
            Symbol canonicalOption,
            decimal leg1Strike,
            decimal leg2Strike,
            DateTime expiration
            )
        {
            CheckCanonicalOptionSymbol(canonicalOption, "BullCallSpread");
            CheckExpirationDate(expiration, "BullCallSpread", nameof(expiration));

            if (leg1Strike >= leg2Strike)
            {
                throw new ArgumentException("BullCallSpread: leg1Strike must be less than leg2Strike", "leg1Strike, leg2Strike");
            }

            return new OptionStrategy
            {
                Name = OptionStrategyDefinitions.BullCallSpread.Name,
                Underlying = canonicalOption.Underlying,
                CanonicalOption = canonicalOption,
                OptionLegs = new List<OptionStrategy.OptionLegData>
                {
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Call, Strike = leg1Strike, Quantity = 1, Expiration = expiration
                    },
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Call, Strike = leg2Strike, Quantity = -1, Expiration = expiration
                    }
                }
            };
        }

        /// <summary>
        /// Method creates new Bull Put Spread strategy, that consists of two puts with the same expiration but different strikes.
        /// The strike price of the short put is above the strike of the long put. This is a credit spread.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="leg1Strike">The strike price of the short put</param>
        /// <param name="leg2Strike">The strike price of the long put</param>
        /// <param name="expiration">Option expiration date</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy BullPutSpread(
            Symbol canonicalOption,
            decimal leg1Strike,
            decimal leg2Strike,
            DateTime expiration
            )
        {
            CheckCanonicalOptionSymbol(canonicalOption, "BullPutSpread");
            CheckExpirationDate(expiration, "BullPutSpread", nameof(expiration));

            if (leg1Strike <= leg2Strike)
            {
                throw new ArgumentException("BullPutSpread: leg1Strike must be greater than leg2Strike", "leg1Strike, leg2Strike");
            }

            return new OptionStrategy
            {
                Name = OptionStrategyDefinitions.BullPutSpread.Name,
                Underlying = canonicalOption.Underlying,
                CanonicalOption = canonicalOption,
                OptionLegs = new List<OptionStrategy.OptionLegData>
                {
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Put, Strike = leg1Strike, Quantity = -1, Expiration = expiration
                    },
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Put, Strike = leg2Strike, Quantity = 1,
                        Expiration = expiration
                    }
                }
            };
        }

        /// <summary>
        /// Method creates new Straddle strategy, that is a combination of buying a call and buying a put, both with the same strike price and expiration.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="strike">The strike price of the both legs</param>
        /// <param name="expiration">Option expiration date</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy Straddle(Symbol canonicalOption, decimal strike, DateTime expiration)
        {
            CheckCanonicalOptionSymbol(canonicalOption, "Straddle");
            CheckExpirationDate(expiration, "Straddle", nameof(expiration));

            return new OptionStrategy
            {
                Name = OptionStrategyDefinitions.Straddle.Name,
                Underlying = canonicalOption.Underlying,
                CanonicalOption = canonicalOption,
                OptionLegs = new List<OptionStrategy.OptionLegData>
                {
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Call, Strike = strike, Quantity = 1,
                        Expiration = expiration
                    },
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Put, Strike = strike, Quantity = 1,
                        Expiration = expiration
                    }
                }
            };
        }

        /// <summary>
        /// Creates a Short Straddle strategy that consists of selling a call and a put, both with the same strike price and expiration.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="strike">The strike price for the option contracts</param>
        /// <param name="expiration">The expiration date for the option contracts</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy ShortStraddle(Symbol canonicalOption, decimal strike, DateTime expiration)
        {
            // Since a short straddle is an inverted straddle, we can just use the Straddle method and invert the legs
            return InvertStrategy(Straddle(canonicalOption, strike, expiration), OptionStrategyDefinitions.ShortStraddle.Name);
        }

        /// <summary>
        /// Method creates new Strangle strategy, that buying a call option and a put option with the same expiration date
        /// The strike price of the call is above the strike of the put.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="callLegStrike">The strike price of the long call</param>
        /// <param name="putLegStrike">The strike price of the long put</param>
        /// <param name="expiration">Option expiration date</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy Strangle(
            Symbol canonicalOption,
            decimal callLegStrike,
            decimal putLegStrike,
            DateTime expiration
            )
        {
            CheckCanonicalOptionSymbol(canonicalOption, "Strangle");
            CheckExpirationDate(expiration, "Strangle", nameof(expiration));

            if (callLegStrike <= putLegStrike)
            {
                throw new ArgumentException($"Strangle: {nameof(callLegStrike)} must be greater than {nameof(putLegStrike)}",
                    $"{nameof(callLegStrike)}, {nameof(putLegStrike)}");
            }

            return new OptionStrategy
            {
                Name = OptionStrategyDefinitions.Strangle.Name,
                Underlying = canonicalOption.Underlying,
                CanonicalOption = canonicalOption,
                OptionLegs = new List<OptionStrategy.OptionLegData>
                {
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Call, Strike = callLegStrike, Quantity = 1, Expiration = expiration
                    },
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Put, Strike = putLegStrike, Quantity = 1, Expiration = expiration
                    }
                }
            };
        }

        /// <summary>
        /// Creates a Short Strangle strategy that consists of selling a call and a put, with the same expiration date and
        /// the call strike being above the put strike.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="callLegStrike">The strike price of the short call</param>
        /// <param name="putLegStrike">The strike price of the short put</param>
        /// <param name="expiration">Option expiration date</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy ShortStrangle(Symbol canonicalOption, decimal callLegStrike, decimal putLegStrike, DateTime expiration)
        {
            // Since a short strangle is an inverted strangle, we can just use the Strangle method and invert the legs
            return InvertStrategy(Strangle(canonicalOption, callLegStrike, putLegStrike, expiration), OptionStrategyDefinitions.ShortStrangle.Name);
        }

        /// <summary>
        /// Method creates new Call Butterfly strategy, that consists of two short calls at a middle strike, and one long call each at a lower and upper strike.
        /// The upper and lower strikes must both be equidistant from the middle strike.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="leg1Strike">The upper strike price of the long call</param>
        /// <param name="leg2Strike">The middle strike price of the two short calls</param>
        /// <param name="leg3Strike">The lower strike price of the long call</param>
        /// <param name="expiration">Option expiration date</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy CallButterfly(
            Symbol canonicalOption,
            decimal leg1Strike,
            decimal leg2Strike,
            decimal leg3Strike,
            DateTime expiration
            )
        {
            CheckCanonicalOptionSymbol(canonicalOption, "CallButterfly");
            CheckExpirationDate(expiration, "CallButterfly", nameof(expiration));

            if (leg1Strike <= leg2Strike ||
                leg3Strike >= leg2Strike ||
                leg1Strike - leg2Strike != leg2Strike - leg3Strike)
            {
                throw new ArgumentException("CallButterfly: upper and lower strikes must both be equidistant from the middle strike", "leg1Strike, leg2Strike, leg3Strike");
            }

            return new OptionStrategy
            {
                Name = OptionStrategyDefinitions.ButterflyCall.Name,
                Underlying = canonicalOption.Underlying,
                CanonicalOption = canonicalOption,
                OptionLegs = new List<OptionStrategy.OptionLegData>
                {
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Call, Strike = leg1Strike, Quantity = 1, Expiration = expiration
                    },
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Call, Strike = leg2Strike, Quantity = -2, Expiration = expiration
                    },
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Call, Strike = leg3Strike, Quantity = 1, Expiration = expiration
                    }
                }
            };
        }

        /// <summary>
        /// Method creates new Put Butterfly strategy, that consists of two short puts at a middle strike, and one long put each at a lower and upper strike.
        /// The upper and lower strikes must both be equidistant from the middle strike.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="leg1Strike">The upper strike price of the long put</param>
        /// <param name="leg2Strike">The middle strike price of the two short puts</param>
        /// <param name="leg3Strike">The lower strike price of the long put</param>
        /// <param name="expiration">Option expiration date</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy PutButterfly(
            Symbol canonicalOption,
            decimal leg1Strike,
            decimal leg2Strike,
            decimal leg3Strike,
            DateTime expiration
            )
        {
            CheckCanonicalOptionSymbol(canonicalOption, "PutButterfly");
            CheckExpirationDate(expiration, "PutButterfly", nameof(expiration));

            if (leg1Strike <= leg2Strike ||
                leg3Strike >= leg2Strike ||
                leg1Strike - leg2Strike != leg2Strike - leg3Strike)
            {
                throw new ArgumentException("PutButterfly: upper and lower strikes must both be equidistant from the middle strike", "leg1Strike, leg2Strike, leg3Strike");
            }

            return new OptionStrategy
            {
                Name = OptionStrategyDefinitions.ButterflyPut.Name,
                Underlying = canonicalOption.Underlying,
                CanonicalOption = canonicalOption,
                OptionLegs = new List<OptionStrategy.OptionLegData>
                {
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Put, Strike = leg1Strike, Quantity = 1,
                        Expiration = expiration
                    },
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Put, Strike = leg2Strike, Quantity = -2,
                        Expiration = expiration
                    },
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Put, Strike = leg3Strike, Quantity = 1,
                        Expiration = expiration
                    }
                }
            };
        }

        /// <summary>
        /// Method creates new Call Calendar Spread strategy, that is a short one call option and long a second call option with a more distant expiration.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="strike">The strike price of the both legs</param>
        /// <param name="expiration1">Option expiration near date</param>
        /// <param name="expiration2">Option expiration far date</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy CallCalendarSpread(
            Symbol canonicalOption,
            decimal strike,
            DateTime expiration1,
            DateTime expiration2
            )
        {
            CheckCanonicalOptionSymbol(canonicalOption, "CallCalendarSpread");
            CheckExpirationDate(expiration1, "CallCalendarSpread", nameof(expiration1));
            CheckExpirationDate(expiration2, "CallCalendarSpread", nameof(expiration2));

            if (expiration1 >= expiration2)
            {
                throw new ArgumentException("CallCalendarSpread: near expiration must be less than far expiration", "expiration1, expiration2");
            }

            return new OptionStrategy
            {
                Name = OptionStrategyDefinitions.CallCalendarSpread.Name,
                Underlying = canonicalOption.Underlying,
                CanonicalOption = canonicalOption,
                OptionLegs = new List<OptionStrategy.OptionLegData>
                {
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Call, Strike = strike, Quantity = -1, Expiration = expiration1
                    },
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Call, Strike = strike, Quantity = 1, Expiration = expiration2
                    }
                }
            };
        }

        /// <summary>
        /// Method creates new Put Calendar Spread strategy, that is a short one put option and long a second put option with a more distant expiration.
        /// </summary>
        /// <param name="canonicalOption">Option symbol</param>
        /// <param name="strike">The strike price of the both legs</param>
        /// <param name="expiration1">Option expiration near date</param>
        /// <param name="expiration2">Option expiration far date</param>
        /// <returns>Option strategy specification</returns>
        public static OptionStrategy PutCalendarSpread(
            Symbol canonicalOption,
            decimal strike,
            DateTime expiration1,
            DateTime expiration2
            )
        {
            CheckCanonicalOptionSymbol(canonicalOption, "PutCalendarSpread");
            CheckExpirationDate(expiration1, "PutCalendarSpread", nameof(expiration1));
            CheckExpirationDate(expiration2, "PutCalendarSpread", nameof(expiration2));

            if (expiration1 >= expiration2)
            {
                throw new ArgumentException("PutCalendarSpread: near expiration must be less than far expiration", "expiration1, expiration2");
            }

            return new OptionStrategy
            {
                Name = OptionStrategyDefinitions.PutCalendarSpread.Name,
                Underlying = canonicalOption.Underlying,
                CanonicalOption = canonicalOption,
                OptionLegs = new List<OptionStrategy.OptionLegData>
                {
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Put, Strike = strike, Quantity = -1, Expiration = expiration1
                    },
                    new OptionStrategy.OptionLegData
                    {
                        Right = OptionRight.Put, Strike = strike, Quantity = 1, Expiration = expiration2
                    }
                }
            };
        }

        /// <summary>
        /// Checks that canonical option symbol is valid
        /// </summary>
        private static void CheckCanonicalOptionSymbol(Symbol canonicalOption, string strategyName)
        {
            if (!canonicalOption.HasUnderlying || canonicalOption.ID.StrikePrice != 0.0m)
            {
                throw new ArgumentException($"{strategyName}: canonicalOption must contain canonical option symbol", nameof(canonicalOption));
            }
        }

        /// <summary>
        /// Checks that expiration date is valid
        /// </summary>
        private static void CheckExpirationDate(DateTime expiration, string strategyName, string parameterName)
        {
            if (expiration == DateTime.MaxValue || expiration == DateTime.MinValue)
            {
                throw new ArgumentException($"{strategyName}: expiration must contain expiration date", parameterName);
            }
        }

        /// <summary>
        /// Inverts the given strategy by multiplying all legs' quantities by -1 and changing the strategy name.
        /// </summary>
        private static OptionStrategy InvertStrategy(OptionStrategy strategy, string invertedStrategyName)
        {
            strategy.Name = invertedStrategyName;
            foreach (var leg in strategy.OptionLegs.Cast<OptionStrategy.LegData>().Concat(strategy.UnderlyingLegs))
            {
                leg.Quantity *= -1;
            }

            return strategy;
        }
    }
}
