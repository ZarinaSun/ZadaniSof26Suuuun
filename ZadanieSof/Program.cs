using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ZadanieSof
{
    class Program
    {
        // Константы для типов комнат
        const int MONSTER = 0;
        const int TRAP = 1;
        const int CHEST = 2;
        const int TRADER = 3;
        const int EMPTY = 4;
        const int BOSS = 5;

        // Константы для видов ловушек (новые)
        const int TRAP_DAMAGE = 0;  // Наносит урон
        const int TRAP_GOLD = 1; // Отнимает золото

        static void Main(string[] args)
        {
            Random random = new Random();

            // 1. Карта подземелья
            int[] dungeonMap = new int[10];
            for (int i = 0; i < dungeonMap.Length - 1; i++)
            {
                dungeonMap[i] = random.Next(0, 5); // Случайный выбор типа комнаты
            }
            dungeonMap[9] = BOSS;

            // --- Характеристики игрока ---
            int playerHealth = 100; // (1) Здоровье
            int playerPotions = 3; // (2) Зелья
            int playerGold = 0;   // (3) Золото
            int playerArrows = 5;  // (4) Стрелы
            

            string[] inventory = new string[5]; // (7) Инвентарь
            int inventoryCount = 0;

            int playerExperience = 0; // Опыт игрока
            int playerLevel = 1; // Уровень игрока

            Console.WriteLine("Добро пожаловать в игру, Солнце!");
            Console.WriteLine("Вы начинаете игру со 100 здоровья, 3 зельями, 0 золота и 5 стрелами.");
            Console.WriteLine("У вас в инвентаре есть меч и лук, успехов!");

            for (int roomNumber = 0; roomNumber < dungeonMap.Length; roomNumber++)
            {
                Console.WriteLine($"\nВы вошли в комнату {roomNumber + 1}. Уровень: {playerLevel}");

                switch (dungeonMap[roomNumber])
                {
                    case MONSTER:
                        Console.WriteLine("В этой комнате вас ждет монстр!");
                        playerHealth = FightMonster(playerHealth, ref playerArrows, random, playerLevel);
                        if (playerHealth <= 0)
                        {
                            Console.WriteLine("Вы погибли в бою...");
                            Console.WriteLine("Игра окончена.");
                            return;
                        }
                        playerExperience += random.Next(5, 16); // Монстры дают опыт
                        Console.WriteLine($"Вы получили опыт!  Текущий опыт: {playerExperience}");
                        CheckLevelUp(ref playerLevel, ref playerExperience); // Проверка уровня
                        break;

                    case TRAP:
                        Console.WriteLine("Осторожно! Вы попали в ловушку!");
                        // --- Разные виды ловушек (новое) ---
                        int trapType = random.Next(0, 2); // 0 - урон, 1 - потеря золота

                        if (trapType == TRAP_DAMAGE)
                        {
                            int trapDamage = random.Next(10, 21);
                            playerHealth -= trapDamage;
                            Console.WriteLine($"Ловушка нанесла вам {trapDamage} урона. Ваше здоровье: {playerHealth}");
                            if (playerHealth <= 0)
                            {
                                Console.WriteLine("Вы погибли в ловушке...");
                                Console.WriteLine("Игра окончена.");
                                return;
                            }
                        }
                        else if (trapType == TRAP_GOLD)
                        {
                            int goldLoss = random.Next(5, 16);
                            playerGold -= goldLoss;
                            if (playerGold < 0) playerGold = 0; // Золото не может быть отрицательным
                            Console.WriteLine($"Ловушка забрала у вас {goldLoss} золота. Ваше золото: {playerGold}");
                        }

                        break;

                    case CHEST:
                        Console.WriteLine("Вы нашли сундук! Чтобы открыть его, решите загадку:");
                        string reward = OpenChest(random, ref playerGold, ref playerArrows, inventory, ref inventoryCount);
                        if (reward != null)
                        {
                            Console.WriteLine($"Вы получили: {reward}!");
                        }
                        break;

                    case TRADER:
                        Console.WriteLine("Вы встретили торговца.");
                        // Используем зелья (новое)
                        if (playerPotions > 0 && playerHealth < 100) // Есть зелья и не полное здоровье
                        {
                            Console.WriteLine($"У вас есть {playerPotions} зелья.  Вы хотите использовать зелье чтобы полечиться? (да/нет)");
                            string potionChoice = Console.ReadLine().ToLower();
                            if (potionChoice == "да")
                            {
                                playerPotions--;
                                playerHealth = 100; // Восстанавливаем полное здоровье (можно сделать более сложную логику, но, пожалуйста, не нужно я и так не люблю задание на время)
                                Console.WriteLine($"Вы использовали зелье. Ваше здоровье восстановлено. Осталось зелий: {playerPotions}");
                            }
                        }

                        playerGold = TradeWithTrader(playerGold);
                        break;

                    case EMPTY:
                        Console.WriteLine("Комната пуста. Здесь ничего не происходит.");
                        break;

                    case BOSS:
                        Console.WriteLine("Впереди - финальный босс!");
                        playerHealth = FightBoss(playerHealth, ref playerArrows, random, playerLevel);
                        if (playerHealth <= 0)
                        {
                            Console.WriteLine("Вы проиграли боссу...");
                            Console.WriteLine("Игра окончена.");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("Поздравляем! Вы победили босса и выиграли игру!");
                            return;
                        }
                } // switch (dungeonMap[roomNumber])

                Console.WriteLine($"Ваше здоровье: {playerHealth}, Золото: {playerGold}, Стрелы: {playerArrows}, Зелья: {playerPotions}");

            } // for (int roomNumber = 0; roomNumber < dungeonMap.Length; roomNumber++)

        } // Main

        // Уровни сложности монстров
        static int GetMonsterHealth(Random random, int playerLevel)
        {
            // Монстры становятся сильнее с уровнем игрока
            int baseHealth = 20 + (playerLevel - 1) * 5; // Базовое здоровье + увеличение от уровня
            int randomHealth = random.Next(0, 31);  // Добавляем случайности
            return baseHealth + randomHealth;
        }


        static int FightMonster(int playerHealth, ref int playerArrows, Random random, int playerLevel)
        {
            // 2. Бой с монстром (усложненный)
            // Разная сложность - здоровье монстра зависит от уровня игрока
            int monsterHealth = GetMonsterHealth(random, playerLevel);

            Console.WriteLine($"Появился монстр! Здоровье монстра: {monsterHealth}");

            while (playerHealth > 0 && monsterHealth > 0)
            {
                Console.WriteLine("Выберите оружие: (1) Меч, (2) Лук");
                if (playerArrows <= 0)
                {
                    Console.WriteLine("У вас нет стрел! Можно использовать только меч.");
                }

                int weaponChoice;
                while (!int.TryParse(Console.ReadLine(), out weaponChoice) || (weaponChoice < 1 || weaponChoice > 2) || (weaponChoice == 2 && playerArrows <= 0))
                {
                    Console.WriteLine("Некорректный ввод. Пожалуйста, выберите 1 или 2 (или только 1, если нет стрел).");
                }

                int playerDamage = 0;
                if (weaponChoice == 1) // Меч
                {
                    playerDamage = random.Next(10, 21);
                    Console.WriteLine($"Вы наносите мечом {playerDamage} урона.");
                }
                else // Лук
                {
                    if (playerArrows > 0)
                    {
                        playerDamage = random.Next(5, 16);
                        playerArrows--;
                        Console.WriteLine($"Вы стреляете из лука и наносите {playerDamage} урона. Осталось стрел: {playerArrows}");
                    }
                    else
                    {
                        Console.WriteLine("У вас нет стрел!");
                    }
                }

                monsterHealth -= playerDamage;
                Console.WriteLine($"Здоровье монстра: {monsterHealth}");

                if (monsterHealth <= 0)
                {
                    Console.WriteLine("Вы победили монстра!");
                    return playerHealth;
                }

                //Монстр тоже может быть сильнее с уровнем, но я не стала добавлять это
                int monsterDamage = random.Next(5, 16);
                playerHealth -= monsterDamage;
                Console.WriteLine($"Монстр наносит вам {monsterDamage} урона. Ваше здоровье: {playerHealth}");

                if (playerHealth <= 0)
                {
                    return playerHealth;
                }

            } // while

            return playerHealth;
        } // FightMonster


        // 2. Бой с боссом (вариант, чтобы босса было сложнее победить)
        static int FightBoss(int playerHealth, ref int playerArrows, Random random, int playerLevel)
        {
            int bossHealth = random.Next(80, 121) + (playerLevel - 1) * 10; // Босс еще сильнее с каждым уровнем
            Console.WriteLine($"Здоровье босса: {bossHealth}");

            Console.WriteLine("Босс обрушивает на вас мощный удар!");
            playerHealth -= 15;
            Console.WriteLine($"Вы получили 15 урона. Ваше здоровье: {playerHealth}");

            while (playerHealth > 0 && bossHealth > 0)
            {
                Console.WriteLine("Выберите оружие: (1) Меч, (2) Лук");
                if (playerArrows <= 0)
                {
                    Console.WriteLine("У вас нет стрел! Можно использовать только меч.");
                }

                int weaponChoice;
                while (!int.TryParse(Console.ReadLine(), out weaponChoice) || (weaponChoice < 1 || weaponChoice > 2) || (weaponChoice == 2 && playerArrows <= 0))
                {
                    Console.WriteLine("Некорректный ввод. Пожалуйста, выберите 1 или 2 (или только 1, если нет стрел).");
                }

                int playerDamage = 0;
                if (weaponChoice == 1) // Меч
                {
                    playerDamage = random.Next(5, 11);
                    Console.WriteLine($"Вы наносите мечом {playerDamage} урона.");
                }
                else // Лук
                {
                    if (playerArrows > 0)
                    {
                        playerDamage = random.Next(10, 21);
                        playerArrows -= 2;
                        Console.WriteLine($"Вы стреляете из лука и наносите {playerDamage} урона. Осталось стрел: {playerArrows}");
                    }
                    else
                    {
                        Console.WriteLine("У вас нет стрел!");
                    }
                }

                bossHealth -= playerDamage;
                Console.WriteLine($"Здоровье босса: {bossHealth}");

                if (bossHealth <= 0)
                {
                    Console.WriteLine("Вы победили босса!");
                    return playerHealth;
                }

                //Монстр тоже может быть сильнее с уровнем, но я не стала добавлять это
                int monsterDamage = random.Next(10, 21);
                playerHealth -= monsterDamage;
                Console.WriteLine($"Босс наносит вам {monsterDamage} урона. Ваше здоровье: {playerHealth}");

                if (playerHealth <= 0)
                {
                    return playerHealth;
                }

            } // while

            return playerHealth;
        } // FightMonster

        // Рандомно выбираем из 3 загадок
        static string OpenChest(Random random, ref int playerGold, ref int playerArrows, string[] inventory, ref int inventoryCount)
        {
            int zadadka = random.Next(0, 3);

            switch (zadadka)
            {
                case 0: return Zadadka1(random, ref playerGold, ref playerArrows, inventory, ref inventoryCount);
                case 1: return Zadadka2(random, ref playerGold, ref playerArrows, inventory, ref inventoryCount);
                case 2: return Zadadka3(random, ref playerGold, ref playerArrows, inventory, ref inventoryCount);
                default: return Zadadka1(random, ref playerGold, ref playerArrows, inventory, ref inventoryCount);
            }
        }

        // 4. Сундуки
        static string Zadadka1(Random random, ref int playerGold, ref int playerArrows, string[] inventory, ref int inventoryCount)
        {
            //Генерируем пример
            int a = random.Next(1, 20);
            int b = random.Next(1, 20);
            int op = random.Next(1, 5);

            string operation = "";

            switch (op)
            {
                case 1:
                    operation = "+";
                    break;
                case 2:
                    operation = "-";
                    break;
                case 3:
                    operation = "*";
                    break;
                case 4:
                    operation = "/";
                    break;
            }

            Console.WriteLine($"Решите пример {a} {operation} {b}");

            int otvet = 0;

            switch (op)
            {
                case 1:
                    otvet = a + b;
                    break;
                case 2:
                    otvet = a - b;
                    break;
                case 3:
                    otvet = a * b;
                    break;
                case 4:
                    if (b == 0)
                    {
                        b = 1;
                        operation = "/";
                    }
                    otvet = a / b;
                    break;
            }

            int playerAnswer;
            while (!int.TryParse(Console.ReadLine(), out playerAnswer) || playerAnswer != otvet)
            {
                Console.WriteLine("Неверный ответ. Попробуйте еще раз.");
                Console.WriteLine($"Решите пример {a} {operation} {b}");
            }

            Console.WriteLine("Правильно! Сундук открыт!");

            // Выбираем награду случайным образом
            int rewardType = random.Next(0, 3); // 0 - зелье, 1 - золото, 2 - стрелы

            switch (rewardType)
            {
                case 0:
                    if (inventoryCount < inventory.Length)
                    {
                        inventory[inventoryCount] = "Зелье здоровья";
                        inventoryCount++;
                        return "Зелье здоровья";
                    }
                    else
                    {
                        Console.WriteLine("Инвентарь полон! Вы не можете взять зелье.");
                        return null; // Ничего не получили
                    }

                case 1:
                    int goldAmount = random.Next(20, 51);
                    playerGold += goldAmount;
                    return $"{goldAmount} золота";

                case 2:
                    int arrowAmount = random.Next(5, 11);
                    playerArrows += arrowAmount;
                    return $"{arrowAmount} стрел";

                default:
                    return null;
            }
        } // OpenChest

        static string Zadadka2(Random random, ref int playerGold, ref int playerArrows, string[] inventory, ref int inventoryCount)
        {
            Console.WriteLine($"Сколько будет два плюс два умноженное на два?");
            Console.WriteLine("1. 6");
            Console.WriteLine("2. 8");
            Console.WriteLine("3. 10");

            int playerAnswer;
            while (!int.TryParse(Console.ReadLine(), out playerAnswer) || playerAnswer != 1)
            {
                Console.WriteLine("Неверный ответ. Попробуйте еще раз.");
                Console.WriteLine($"Сколько будет два плюс два умноженное на два?");
                Console.WriteLine("1. 6");
                Console.WriteLine("2. 8");
                Console.WriteLine("3. 10");
            }

            Console.WriteLine("Правильно! Сундук открыт!");

            // Выбираем награду случайным образом
            int rewardType = random.Next(0, 3); // 0 - зелье, 1 - золото, 2 - стрелы

            switch (rewardType)
            {
                case 0:
                    if (inventoryCount < inventory.Length)
                    {
                        inventory[inventoryCount] = "Зелье здоровья";
                        inventoryCount++;
                        return "Зелье здоровья";
                    }
                    else
                    {
                        Console.WriteLine("Инвентарь полон! Вы не можете взять зелье.");
                        return null; // Ничего не получили
                    }

                case 1:
                    int goldAmount = random.Next(20, 51);
                    playerGold += goldAmount;
                    return $"{goldAmount} золота";

                case 2:
                    int arrowAmount = random.Next(5, 11);
                    playerArrows += arrowAmount;
                    return $"{arrowAmount} стрел";

                default:
                    return null;
            }
        } // OpenChest

        static string Zadadka3(Random random, ref int playerGold, ref int playerArrows, string[] inventory, ref int inventoryCount)
        {
            Console.WriteLine($"Назовите число, которое одновременно делится на 2, 3 и 5?");
            Console.WriteLine("1. 10");
            Console.WriteLine("2. 15");
            Console.WriteLine("3. 30");

            int playerAnswer;
            while (!int.TryParse(Console.ReadLine(), out playerAnswer) || playerAnswer != 3)
            {
                Console.WriteLine("Неверный ответ. Попробуйте еще раз.");
                Console.WriteLine($"Назовите число, которое одновременно делится на 2, 3 и 5?");
                Console.WriteLine("1. 10");
                Console.WriteLine("2. 15");
                Console.WriteLine("3. 30");
            }

            Console.WriteLine("Правильно! Сундук открыт!");

            // Выбираем награду случайным образом
            int rewardType = random.Next(0, 3); // 0 - зелье, 1 - золото, 2 - стрелы

            switch (rewardType)
            {
                case 0:
                    if (inventoryCount < inventory.Length)
                    {
                        inventory[inventoryCount] = "Зелье здоровья";
                        inventoryCount++;
                        return "Зелье здоровья";
                    }
                    else
                    {
                        Console.WriteLine("Инвентарь полон! Вы не можете взять зелье.");
                        return null; // Ничего не получили
                    }

                case 1:
                    int goldAmount = random.Next(20, 51);
                    playerGold += goldAmount;
                    return $"{goldAmount} золота";

                case 2:
                    int arrowAmount = random.Next(5, 11);
                    playerArrows += arrowAmount;
                    return $"{arrowAmount} стрел";

                default:
                    return null;
            }
        } // OpenChest

        // 5. Торговец
        static int TradeWithTrader(int playerGold)
        {
            Console.WriteLine($"У вас {playerGold} золота. Хотите купить зелье здоровья за 30 золота? (да/нет)");
            string choice = Console.ReadLine().ToLower();

            if (choice == "да")
            {
                if (playerGold >= 30)
                {
                    playerGold -= 30;
                    Console.WriteLine("Вы купили зелье здоровья. Спасибо за покупку!");
                }
                else
                {
                    Console.WriteLine("У вас недостаточно золота.");
                }
            }
            else
            {
                Console.WriteLine("До свидания!");
            }

            return playerGold;
        } // TradeWithTrader

        //Система опыта и уровней
        static void CheckLevelUp(ref int playerLevel, ref int playerExperience)
        {
            int experienceToNextLevel = playerLevel * 50; // Пример: для 2 уровня нужно 100 опыта, для 3 - 150 и т.д.

            if (playerExperience >= experienceToNextLevel)
            {
                playerLevel++;
                playerExperience -= experienceToNextLevel;
                Console.WriteLine($"Уровень повышен! Теперь ваш уровень: {playerLevel}");
            }
        }
    } // class 
}
