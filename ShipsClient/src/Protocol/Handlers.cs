using System;
using System.Windows;
using ShipsClient.Auth;
using ShipsClient.BattleWindow;
using ShipsClient.Enums;
using ShipsClient.Game;
using ShipsClient.MainWindow;
using ShipsClient.Messages;
using ShipsClient.Protocol.Parser;

namespace ShipsClient.Protocol
{
    public static class Handlers
    {
        [Parser(Opcode.SMSG_AUTH_RESPONSE)]
        public static void HandleAuthResponse(Packet packet)
        {
            var responseCode = (AuthResponse) packet.ReadUInt8();
            switch (responseCode)
            {
                case AuthResponse.AUTH_RESPONSE_UNKNOWN_USER:
                {
                    AuthWindow.Form.ErrorMessage = AuthMessages.ErrorMessages[2];
                    break;
                }
                case AuthResponse.AUTH_RESPONSE_UNKNOWN_ERROR:
                {
                    AuthWindow.Form.ErrorMessage = AuthMessages.ErrorMessages[3];
                    break;
                }
                case AuthResponse.AUTH_RESPONSE_SUCCESS:
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        AuthWindow.Form.ResetControls();
                        MainWindow.MainWindow window = new MainWindow.MainWindow {Owner = AuthWindow.Form};
                        window.Show();
                        AuthWindow.Form.Visibility = Visibility.Hidden;
                    }));
                    break;
                }
            }

            // Включение кнопки входа
            AuthWindow.Form.AuthButtonEnable = true;
        }

        [Parser(Opcode.SMSG_REGISTRATION_RESPONSE)]
        public static void HandleRegistrationResponse(Packet packet)
        {
            RegistrationResponse responseCode = (RegistrationResponse) packet.ReadUInt8();
            switch (responseCode)
            {
                case RegistrationResponse.REG_RESPONSE_HERE_USER:
                {
                    RegistrationWindow.Form.ErrorMessage = RegMessages.ErrorMessages[1];
                    break;
                }
                case RegistrationResponse.REG_RESPONSE_UNKNOWN_ERROR:
                {
                    RegistrationWindow.Form.ErrorMessage = RegMessages.ErrorMessages[2];
                    break;
                }
                case RegistrationResponse.REG_RESPONSE_SUCCESS:
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AuthWindow.Form.ResetControls();
                        var window = new MainWindow.MainWindow {Owner = AuthWindow.Form};
                        window.Show();
                        RegistrationWindow.Form.Close();
                    });
                    break;
                }
            }

            // Включение кнопки регистрации
            RegistrationWindow.Form.RegButtonEnable = true;
        }

        [Parser(Opcode.SMSG_PROFILE_RESPONSE)]
        public static void HandleProfileResponse(Packet packet)
        {
            string username = packet.ReadUTF8String();

            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.MainWindow.Form.ProfileButton = username;
            });
        }

        [Parser(Opcode.SMSG_GET_GAMES_RESPONSE)]
        public static void HandleGetGamesResponse(Packet packet)
        {
            int count = packet.ReadInt32();
            if (count == 0) // На сервер не оказалось игр к которым можно подключиться
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MainWindow.MainWindow.Form.EmptyGames();
                });
                return;
            }

            var battles = new int[count];
            for (int i = 0; i < count; ++i)
                battles[i] = packet.ReadInt32();

            Application.Current.Dispatcher.Invoke(() =>
            {
                var rnd = new Random();
                MainWindow.MainWindow.Form.JoinBattle(battles[rnd.Next(count)]);
            });
        }

        [Parser(Opcode.SMSG_BATTLE_INITIAL_BATTLE)]
        public static void HandleBattleInitialBattle(Packet packet)
        {
            var battleId = packet.ReadInt32();
            var responseCode = (BattleResponse) packet.ReadUInt8();
            Application.Current.Dispatcher.Invoke(() =>
            {
                PreBattleWindow.Form.InitialBattleWindow(battleId, responseCode, "", "Ожид. против.");
            });
        }

        [Parser(Opcode.SMSG_BATTLE_SHOT_RESULT)]
        public static void HandleBattleShotResult(Packet packet)
        {
            var result = packet.ReadUInt8();
            var x = packet.ReadUInt8();
            var y = packet.ReadUInt8();
            Application.Current.Dispatcher.Invoke(() =>
            {
                BattleWindow.BattleWindow.Form.ShotResult(x, y, (ShotResult) result);
            });
        }

        [Parser(Opcode.SMSG_BATTLE_CAN_SHOT)]
        public static void HandleBattleCanShot(Packet packet)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                BattleWindow.BattleWindow.Form.CanShot = true;
            });
        }

        [Parser(Opcode.SMSG_BATTLE_RESPONSE)]
        public static void HandleBattleResponse(Packet packet)
        {
            var responseCode = (BattleResponse) packet.ReadUInt8();
            switch (responseCode)
            {
                case BattleResponse.BATTLE_RESPONSE_JOIN_SUCCESS:
                {
                    var battleId = packet.ReadInt32();
                    var myUsername = packet.ReadUTF8String();
                    var opUsername = packet.ReadUTF8String();
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PreBattleWindow.Form.InitialBattleWindow(battleId, responseCode, myUsername,
                            opUsername);
                    });
                    break;
                }
            }
        }

        [Parser(Opcode.SMSG_BATTLE_OPONENT_JOINED)]
        public static void HandleBattleOponentJoined(Packet packet)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var myName = packet.ReadUTF8String();
                var oponentName = packet.ReadUTF8String();
                BattleWindow.BattleWindow.Form.MyTextBlock = myName;
                BattleWindow.BattleWindow.Form.OponentTextBlock = oponentName;
                BattleWindow.BattleWindow.Form.StartBattle();
            });
        }

        [Parser(Opcode.SMSG_BATTLE_SHIP_DROWNED)]
        public static void HandleBattleShipDrowned(Packet packet)
        {
            var count = packet.ReadUInt8();
            Application.Current.Dispatcher.Invoke(() =>
            {
                for (int i = 0; i < count; ++i)
                {
                    var x = packet.ReadUInt8();
                    var y = packet.ReadUInt8();
                    BattleWindow.BattleWindow.Form.ShotResult(x, y, ShotResult.SHOT_RESULT_RESET_CELL);
                }

                var length = packet.ReadUInt8();
                var orientation = packet.ReadUInt8();
                var shipX = packet.ReadUInt8();
                var shipY = packet.ReadUInt8();
                BattleWindow.BattleWindow.Form.ShipDrowned(new Ship(length, (ShipOrientation)orientation, shipX, shipY, length));
            });
        }

        [Parser(Opcode.SMSG_BATTLE_OPONENT_LEAVE)]
        public static void HandleBattleOponentLeave(Packet packet)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                BattleWindow.BattleWindow.Form.EndBattle(true);
            });
        }

        [Parser(Opcode.SMSG_BATTLE_FINISH)]
        public static void HandleBattleFinish(Packet packet)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                BattleWindow.BattleWindow.Form.EndBattle(false, packet.ReadUInt8() == 1 ? true : false);
            });
        }

        [Parser(Opcode.SMSG_BATTLE_OPONENT_SHOT_RESULT)]
        public static void HandleBattleOponentShotResult(Packet packet)
        {
            var result = (ShotResult)packet.ReadUInt8();
            var x = packet.ReadUInt8();
            var y = packet.ReadUInt8();
            Application.Current.Dispatcher.Invoke(() =>
            {
                BattleWindow.BattleWindow.Form.UpdateBoard(x, y, result);
            });
        }

        [Parser(Opcode.SMSG_GET_STATISTICS_RESPONSE)]
        public static void HandleGetStatisticsResponse(Packet packet)
        {
            var lastBattle = packet.ReadUInt32();
            var wins = packet.ReadUInt16();
            var loose = packet.ReadUInt16();
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (lastBattle == 0)
                {
                    new NotificationWindow.NotificationWindow("Статистика", "Для вас нету статистики. Возможно вы еще не сыграли ни одного боя...").ShowDialog();
                    return;
                }

                var window = new StatisticsWindow(lastBattle, wins, loose) { Owner = MainWindow.MainWindow.Form };
                window.ShowDialog();
            });
        }

        [Parser(Opcode.SMSG_CHAT_MESSAGE)]
        public static void HandleChatMessage(Packet packet)
        {
            var username = packet.ReadUTF8String();
            var text = packet.ReadUTF8String();

            Application.Current.Dispatcher.Invoke(() =>
            {
                BattleWindow.BattleWindow.Form.RecivieMessage(username, text);
            });
        }
    }
}
