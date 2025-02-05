﻿using Google.Protobuf.Collections;
using Hashgraph;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Proto
{
    internal static class AssessedCustomFeeExtensions
    {
        private static ReadOnlyCollection<CommissionTransfer> EMPTY_RESULT = new List<CommissionTransfer>().AsReadOnly();
        internal static ReadOnlyCollection<CommissionTransfer> AsCommissionTransferList(this RepeatedField<AssessedCustomFee> list)
        {
            if (list != null && list.Count > 0)
            {
                return list.Select(fee => new CommissionTransfer(fee)).ToList().AsReadOnly();
            }
            return EMPTY_RESULT;
        }
    }
}
