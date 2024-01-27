using NUnit.Framework;
using Npgsql;
using System;
using System.Data;
using byenipinar_MTCG.GameClasses;

namespace byenipinar_MTCG.Tests
{
    [TestFixture]
    public class DataBattleTradeTests
    {
        private DataBattleTrade dataBattleTrade;
        private Data data;

        [SetUp]
        public void Setup()
        {
            dataBattleTrade = new DataBattleTrade();
            data = new Data();
            data.DataAutomation();
        }

        [Test]
        public void GetTradingDataJson_ShouldReturnValidJson()
        {
            string result = dataBattleTrade.GetTradingDataJson();

            Assert.IsNotNull(result);
        }

        [Test]
        public void IdExist_ExistingId_ShouldReturnTrue()
        {
            string existingId = "6cd85277-4590-49d4-b0cf-ba0a921faad0";

            bool result = dataBattleTrade.IdExist(existingId);

            Assert.IsFalse(result);
        }

        [Test]
        public void IdExist_NonExistingId_ShouldReturnFalse()
        {
            string nonExistingId = "nonExistingId";

            bool result = dataBattleTrade.IdExist(nonExistingId);

            Assert.IsFalse(result);
        }

        [Test]
        public void DoesUserExist_UserExists_ShouldReturnTrue()
        {
            User testUser = new User { Username = "kienboec", Password = "daniel" };
            data = new Data(testUser);
            data.AddUser();

            bool result = data.DoesUserExist(testUser.Username);

            Assert.IsTrue(result);
        }

        [Test]
        public void DoesUserExist_UserDoesNotExist_ShouldReturnFalse()
        {
            string nonExistingUsername = "nonExistingUser";

            bool result = data.DoesUserExist(nonExistingUsername);

            Assert.IsFalse(result);
        }

        [Test]
        public void VerifyUserCredentials_CorrectCredentials_ShouldReturnTrue()
        {
            User testUser = new User { Username = "testUser", Password = "password123" };
            data = new Data(testUser);
            data.AddUser();

            bool result = data.VerifyUserCredentials(testUser);

            Assert.IsTrue(result);
        }

        [Test]
        public void VerifyUserCredentials_IncorrectPassword_ShouldReturnFalse()
        {
            User testUser = new User { Username = "testUser", Password = "password123" };
            data = new Data(testUser);
            data.AddUser();
            testUser.Password = "incorrectPassword";

            bool result = data.VerifyUserCredentials(testUser);

            Assert.IsFalse(result);
        }

        [Test]
        public void VerifyUserCredentials_UserDoesNotExist_ShouldReturnFalse()
        {
            User nonExistingUser = new User { Username = "nonExistingUser", Password = "password123" };

            bool result = data.VerifyUserCredentials(nonExistingUser);

            Assert.IsFalse(result);
        }

        [Test]
        public void GetUsername_ValidToken_ShouldReturnUsername()
        {
            string validToken = "testUser-mtcgToken";

            string result = data.GetUsername(validToken);

            Assert.AreEqual("testUser", result);
        }

        [Test]
        public void GetUsername_InvalidToken_ShouldReturnEmptyString()
        {
            string invalidToken = "invalidToken";

            string result = data.GetUsername(invalidToken);

            Assert.AreEqual("", result);
        }

        [Test]
        public void GetAvailablePackages_ShouldReturnListOfPackageIds()
        {
            List<int> result = data.GetAvailablePackages();

            Assert.IsNotNull(result);
        }

        [Test]
        public void TokenExist_NonExistingToken_ShouldReturnFalse()
        {
            string nonExistingToken = "nonExistingToken";

            bool result = data.TokenExist(nonExistingToken);

            Assert.IsFalse(result);
        }

        [Test]
        public void GetCardDataJson_ValidToken_ShouldReturnValidJson()
        {
            string validToken = "kienboec-mtcgToken";
            data.SpeicherePackages("[]");

            string result = data.GetCardDataJson(validToken);

            Assert.IsNotNull(result);
        }


        [Test]
        public void DeleteDeckFromUser_NonExistingUsername_ShouldReturnFalse()
        {
            string nonExistingUsername = "nonExistingUser";

            bool result = data.DeleteDeckFromUser(nonExistingUsername);

            Assert.IsFalse(result);
        }

        [Test]
        public void ExtractUsernameFromRequest_ValidRequest_ShouldExtractUsername()
        {
            string validRequest = "GET /users/username123 HTTP/1.1";

            string result = data.ExtractUsernameFromRequest(validRequest);

            Assert.AreEqual("username123", result);
        }

        [Test]
        public void ExtractUsernameFromRequest_InvalidRequest_ShouldReturnEmptyString()
        {
            string invalidRequest = "GET /invalidRequest HTTP/1.1";

            string result = data.ExtractUsernameFromRequest(invalidRequest);

            Assert.AreEqual(string.Empty, result);
        }

        [Test]
        public void GetUserDataJson_NonExistingUsername_ShouldReturnEmptyJson()
        {
            string nonExistingUsername = "nonExistingUser";

            string result = data.GetUserDataJson(nonExistingUsername);

            Assert.AreEqual("{}", result);
        }

     

        [Test]
        public void GetUserStatisticsJson_NonExistingToken_ShouldReturnEmptyJson()
        {
            string nonExistingToken = "nonExistingToken";

            string result = data.GetUserStatisticsJson(nonExistingToken);

            Assert.AreEqual("{}", result);
        }

        [Test]
        public void ExtractAuthorizationToken_ValidRequestWithToken_ShouldReturnToken()
        {
            string validRequest = "GET /path HTTP/1.1\r\nAuthorization: Bearer ValidToken\r\n";

            string result = data.ExtractAuthorizationToken(validRequest);

            Assert.AreEqual("ValidToken", result);
        }

        [Test]
        public void ExtractAuthorizationToken_InvalidRequest_ShouldReturnEmptyString()
        {
            string invalidRequest = "GET /path HTTP/1.1\r\n";

            string result = data.ExtractAuthorizationToken(invalidRequest);

            Assert.IsEmpty(result);
        }

        [Test]
        public void UpdatePackageIdForCard_Should_Return_False_For_NonExisting_Card()
        {
            // Arrange
            string cardId = "nonExistingCardId";
            int newPackageId = 123;

            // Act
            bool result = dataBattleTrade.UpdatePackageIdForCard(cardId, newPackageId);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void DoesCardExistInTrading_Should_Return_False_For_NonExisting_Card_In_Trading()
        {
            // Arrange
            string cardId = "nonExistingCardId";

            // Act
            bool result = dataBattleTrade.DoesCardExistInTrading(cardId);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void InsertTrade_Should_Insert_Trade_Successfully()
        {
            // Arrange
            string cardToTrade = "1cb6ab86-bdb2-47e5-b6e4-68c5ab389334";
            string id = "6cd85277-4590-49d4-b0cf-ba0a921faad0";
            string cardType = "monster";
            double minimumDamage = 15;
            string username = "kienboec";

            // Act
            Assert.DoesNotThrow(() => dataBattleTrade.InsertTrade(cardToTrade, id, cardType, minimumDamage, username));

            // Optionally: Verify that the trade was actually inserted by querying the database or using another method
        }

        [Test]
        public void DeleteTrade_Should_Delete_Trade_Successfully()
        {
            // Arrange
            string tradeId = "1cb6ab86-bdb2-47e5-b6e4-68c5ab389334";

            // Act
            Assert.DoesNotThrow(() => dataBattleTrade.DeleteTrade(tradeId));

            // Optionally: Verify that the trade was actually deleted by querying the database or using another method
        }

    }
}
