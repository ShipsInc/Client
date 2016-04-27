using System;
using System.Windows;
using ShipsClient.Auth;
using ShipsClient.Common;
using ShipsClient.Enums;
using ShipsClient.Game;
using ShipsClient.Messages;

namespace ShipsClient.Protocol
{
    public class Handlers
    {
        public static void SelectHandler(Packet packet)
        {
            switch ((Opcodes) packet.Opcode)
            {
                case Opcodes.SMSG_AUTH_RESPONSE:
                    HandleAuthResponse(packet);
                    break;
                case Opcodes.SMSG_REGISTRATION_RESPONSE:
                    HandleAuthRegistration(packet);
                    break;
                case Opcodes.SMSG_PROFILE_RESPONSE:
                    HandleProfileResponse(packet);
                    break;
                case Opcodes.SMSG_GET_GAMES_RESPONSE:
                    HandleGetGamesResponse(packet);
                    break;
                case Opcodes.SMSG_BATTLE_INITIAL_BATTLE:
                    HandleBattleInitialBattle(packet);
                    break;
                case Opcodes.SMSG_BATTLE_SHOT_RESULT:
                    HandleBattleShotResult(packet);
                    break;
                case Opcodes.SMSG_BATTLE_CAN_SHOT:
                    HandleBattleCanShot(packet);
                    break;
                case Opcodes.SMSG_BATTLE_RESPONSE:
                    HandleBattleResponse(packet);
                    break;
                case Opcodes.SMSG_BATTLE_OPONENT_JOINED:
                    HandleBattleOponentJoined(packet);
                    break;
                case Opcodes.SMSG_BATTLE_SHIP_DROWNED:
                    HandleBattleShipDrowned(packet);
                    break;
                case Opcodes.SMSG_BATTLE_OPONENT_LEAVE:
                    HandleBattleOponentLeave(packet);
                    break;
                case Opcodes.SMSG_BATTLE_FINISH:
                    HandleBattleFinish(packet);
                    break;
                case Opcodes.SMSG_BATTLE_OPONENT_SHOT_RESULT:
                    HandleBattleOponentShotResult(packet);
                    break;
                case Opcodes.SMSG_KEEP_ALIVE:
                    break;
                default:
                    Console.WriteLine($"SelectHandler: Not found handler for opcode {packet.Opcode.ToString()}");
                    break;
            }
        }

        private static void HandleAuthResponse(Packet packet)
        {
            AuthResponse responseCode = (AuthResponse) packet.ReadUInt8();
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
                        MainWindow.MainWindow window = new MainWindow.MainWindow();
                        window.Owner = AuthWindow.Form;
                        window.Show();
                        AuthWindow.Form.Visibility = Visibility.Hidden;
                    }));
                    break;
                }
                default:
                    break;
            }

            // Включение кнопки входа
            AuthWindow.Form.AuthButtonEnable = true;
        }

        private static void HandleAuthRegistration(Packet packet)
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
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        AuthWindow.Form.ResetControls();
                        MainWindow.MainWindow window = new MainWindow.MainWindow();
                        window.Owner = AuthWindow.Form;
                        window.Show();
                        RegistrationWindow.Form.Close();
                    }));
                    break;
                }
                default:
                    break;
            }

            // Включение кнопки регистрации
            RegistrationWindow.Form.RegButtonEnable = true;
        }

        private static void HandleProfileResponse(Packet packet)
        {
            string username = packet.ReadUTF8String();

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MainWindow.MainWindow.Form.ProfileButton = username;
            }));
        }

        private static void HandleGetGamesResponse(Packet packet)
        {
            int count = packet.ReadInt32();
            if (count == 0) // На сервер не оказалось игр к которым можно подключиться
            {
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MainWindow.MainWindow.Form.EmptyGames();
                }));
                return;
            }

            int[] battles = new int[count];
            for (int i = 0; i < count; ++i)
                battles[i] = packet.ReadInt32();

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Random rnd = new Random();
                MainWindow.MainWindow.Form.JoinBattle(battles[rnd.Next(count)]);
            }));
        }

        private static void HandleBattleInitialBattle(Packet packet)
        {
            var battleId = packet.ReadInt32();
            var responseCode = (BattleResponse) packet.ReadUInt8();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                PreBattleWindow.PreBattleWindow.Form.InitialBattleWindow(battleId, responseCode, "",
                    "Ожидайте противника");
            }));
        }

        private static void HandleBattleShotResult(Packet packet)
        {
            var result = packet.ReadUInt8();
            var x = packet.ReadUInt8();
            var y = packet.ReadUInt8();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                BattleWindow.BattleWindow.Form.ShotResult(x, y, (ShotResult) result);
            }));
        }

        private static void HandleBattleCanShot(Packet packet)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                BattleWindow.BattleWindow.Form.CanShot = true;
            }));
        }

        private static void HandleBattleResponse(Packet packet)
        {
            var responseCode = (BattleResponse) packet.ReadUInt8();
            switch (responseCode)
            {
                case BattleResponse.BATTLE_RESPONSE_JOIN_SUCCESS:
                {
                    var battleId = packet.ReadInt32();
                    var myUsername = packet.ReadUTF8String();
                    var opUsername = packet.ReadUTF8String();
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        PreBattleWindow.PreBattleWindow.Form.InitialBattleWindow(battleId, responseCode, myUsername,
                            opUsername);
                    }));
                    break;
                }
                default:
                    break;
            }
        }

        private static void HandleBattleOponentJoined(Packet packet)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var myName = packet.ReadUTF8String();
                var oponentName = packet.ReadUTF8String();
                BattleWindow.BattleWindow.Form.MyTextBlock = myName;
                BattleWindow.BattleWindow.Form.OponentTextBlock = oponentName;
                BattleWindow.BattleWindow.Form.StartBattle();
            }));
        }

        private static void HandleBattleShipDrowned(Packet packet)
        {
            var count = packet.ReadUInt8();
            Application.Current.Dispatcher.Invoke(new Action(() =>
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
            }));
        }

        private static void HandleBattleOponentLeave(Packet packet)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                BattleWindow.BattleWindow.Form.EndBattle(true);
            }));
        }

        private static void HandleBattleFinish(Packet packet)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                BattleWindow.BattleWindow.Form.EndBattle(false, packet.ReadUInt8() == 1 ? true : false);
            }));
        }

        private static void HandleBattleOponentShotResult(Packet packet)
        {
            var result = (ShotResult)packet.ReadUInt8();
            var x = packet.ReadUInt8();
            var y = packet.ReadUInt8();
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                BattleWindow.BattleWindow.Form.UpdateBoard(x, y, result);
            }));
        }
    }
}
