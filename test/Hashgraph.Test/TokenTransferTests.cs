﻿using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests
{
    public class TokenTransferTests
    {
        [Fact(DisplayName = "TokenTransfer: Can Create Token Transfer Object from Address")]
        public void CanCreateTokenTransferObjectFromAddress()
        {
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt = new TokenTransfer(token, address, amount);
            Assert.Equal(new TokenIdentifier(token), tt.Token);
            Assert.Equal(address, tt.Address);
            Assert.Equal(amount, tt.Amount);
        }
        [Fact(DisplayName = "TokenTransfer: Can Create Token Transfer Object from Symbol")]
        public void CanCreateTokenTransferObjectFromSymbol()
        {
            var symbol = Generator.UppercaseAlphaCode(20);
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt = new TokenTransfer(symbol, address, amount);
            Assert.Equal(new TokenIdentifier(symbol), tt.Token);
            Assert.Equal(address, tt.Address);
            Assert.Equal(amount, tt.Amount);
        }
        [Fact(DisplayName = "TokenTransfer: Equivalent TokenTransfers are considered Equal")]
        public void EquivalentTokenTransferAreConsideredEqual()
        {
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt1 = new TokenTransfer(token, address, amount);
            var tt2 = new TokenTransfer(token, address, amount);
            Assert.Equal(tt1, tt2);
            Assert.True(tt1 == tt2);
            Assert.False(tt1 != tt2);
            Assert.True(tt1.Equals(tt2));
            Assert.True(tt2.Equals(tt1));
            Assert.True(null as TokenTransfer == null as TokenTransfer);
        }
        [Fact(DisplayName = "TokenTransfer: Disimilar TokenTransfers are not considered Equal")]
        public void DisimilarTokenTransfersAreNotConsideredEqual()
        {
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            var other = new Address(0, 0, Generator.Integer(500, 600));
            long amount = Generator.Integer(500, 600);
            var tt = new TokenTransfer(token, address, amount);
            Assert.NotEqual(tt, new TokenTransfer(token, other, amount));
            Assert.NotEqual(tt, new TokenTransfer(other, address, amount));
            Assert.NotEqual(tt, new TokenTransfer(token, address, amount + 1));
            Assert.False(tt == new TokenTransfer(token, address, amount + 1));
            Assert.True(tt != new TokenTransfer(token, address, amount + 1));
            Assert.False(tt.Equals(new TokenTransfer(other, address, amount)));
            Assert.False(tt.Equals(new TokenTransfer(token, other, amount)));
            Assert.False(tt.Equals(new TokenTransfer(token, address, amount + 1)));
        }
        [Fact(DisplayName = "TokenTransfer: Comparing with null are not considered equal.")]
        public void NullTokenTransfersAreNotConsideredEqual()
        {
            object asNull = null;
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt = new TokenTransfer(token, address, amount);
            Assert.False(tt == null);
            Assert.False(null == tt);
            Assert.True(tt != null);
            Assert.False(tt.Equals(null));
            Assert.False(tt.Equals(asNull));
        }
        [Fact(DisplayName = "TokenTransfer: Comparing with other objects are not considered equal.")]
        public void OtherObjectsAreNotConsideredEqual()
        {
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt = new TokenTransfer(token, address, amount);
            Assert.False(tt.Equals("Something that is not an TokenTransfer"));
        }
        [Fact(DisplayName = "TokenTransfer: TokenTransfer cast as object still considered equivalent.")]
        public void TokenTransferCastAsObjectIsconsideredEqual()
        {
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt = new TokenTransfer(token, address, amount);
            object equivalent = new TokenTransfer(token, address, amount);
            Assert.True(tt.Equals(equivalent));
            Assert.True(equivalent.Equals(tt));
        }
        [Fact(DisplayName = "TokenTransfer: TokenTransfer as objects but reference equal are same.")]
        public void ReferenceEqualIsconsideredEqual()
        {
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt = new TokenTransfer(token, address, amount);
            object reference = tt;
            Assert.True(tt.Equals(reference));
            Assert.True(reference.Equals(tt));
        }
        [Fact(DisplayName = "TokenTransfer: Can Create New Token Transfer Records with the Add Method")]
        public void CombinAmountsForNewTokenTransferRecord()
        {
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt1 = new TokenTransfer(token, address, amount);
            var tt2 = tt1.Add(amount);
            var tt3 = tt2.Add(-amount);
            Assert.Equal(amount, tt1.Amount);
            Assert.Equal(amount * 2, tt2.Amount);
            Assert.Equal(amount, tt3.Amount);
            Assert.True(tt1.Equals(tt3));
            Assert.True(tt3.Equals(tt1));
        }
    }
}
