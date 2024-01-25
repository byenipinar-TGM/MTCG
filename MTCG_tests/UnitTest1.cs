using NUnit.Framework;
using byenipinar_MTCG.GameClasses;
using MonsterTradingCardGame.Repository;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Text;
using System.Security.AccessControl;
using MonsterTradingCardGameTest;
namespace MTCG_tests
{
    [TestFixture]
    public class Tests
    {
        private DataBattleTrade dataBattleTrade;

        [SetUp]
        public void Setup()
        {
            dataBattleTrade = new DataBattleTrade();
        }

        [Test]
        public void GetTradingDataJson_ShouldReturnValidJsonString()
        {
            // Arrange
            // Assuming that there is some data in the tradings table for testing

            // Act
            string result = dataBattleTrade.GetTradingDataJson();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.StartsWith("["));
            Assert.IsTrue(result.EndsWith("]"));
        }

        [Test]
        public void IdExist_ExistingId_ShouldReturnTrue()
        {
            // Arrange
            string existingId = "existingId";

            // Act
            bool result = dataBattleTrade.IdExist(existingId);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void IdExist_NonExistingId_ShouldReturnFalse()
        {
            // Arrange
            string nonExistingId = "nonExistingId";

            // Act
            bool result = dataBattleTrade.IdExist(nonExistingId);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void UpdatePackageIdForCard_ShouldUpdatePackageId()
        {
            // Arrange
            string cardIdToUpdate = "cardIdToUpdate";
            int newPackageId = 123;

            // Act
            bool result = dataBattleTrade.UpdatePackageIdForCard(cardIdToUpdate, newPackageId);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void DoesCardExistInTrading_ExistingCard_ShouldReturnTrue()
        {
            // Arrange
            string existingCardId = "existingCardId";

            // Act
            bool result = dataBattleTrade.DoesCardExistInTrading(existingCardId);

            // Assert
            Assert.IsTrue(result);
        }

        // Add more tests as needed
    }
}