using System;
using System.Threading.Tasks;
using System.IO;
namespace Project3___LifeSim
{
    class Program
    {
        /*  Dokuz Eylul University Dept. of Computer Engineering
         *  CME1101 Project Based Learning I - Project 3 - LifeSim
         *  Created by Muhammet Tayyip Muslu 
         *  Version : 1.2
         *  Date : 20.12.2016
        */
        private static char dead = '.';
        private static char live = 'o';
        private static char[,] board = new char[16, 32]; // Simulation screen
        private static int[,] neigbours = new int[16, 32]; // for evolation
        private static char[,] particles = new char[4, 9]; // Particle (Q,W,E are static, R is dynamic)
        private static int cursorX = 29, cursorY = 7; // Middle of the board
        private static byte step = 0; // Simulation generation steps
        private static bool debug = false, error = false, save = true; // Debug mode for catching errors
        private static int debugLiveCount = 0, debugDeadCount = 0;
        private static string savePath = "save.txt";
        static void Main(string[] arggs)
        {
            Console.Title = "LifeSim v1.2";
            if(debug) Console.SetWindowSize(89, 27); else Console.SetWindowSize(92, 19);
            Intro(); // intro part
            CreateParticle('q'); CreateParticle('w');
            CreateParticle('e'); CreateParticle('r');// Particles are created
            DravScreen();// First Create Empty Screen
            KeyControl(); // simulation loop
        }
        private static void Intro()
        {
            Console.WriteLine(@"         __        __   ______              ______   ______  __       __ ");
            Console.WriteLine(@"        /  |      /  | /      \            /      \ /      |/  \     /  |");
            Console.WriteLine(@"        $$ |      $$/ /$$$$$$  |  ______  /$$$$$$  |$$$$$$/ $$  \   /$$ |");
            Console.WriteLine(@"        $$ |      /  |$$ |_ $$/  /      \ $$ \__$$/   $$ |  $$$  \ /$$$ |");
            Console.WriteLine(@"        $$ |      $$ |$$   |    /$$$$$$  |$$      \   $$ |  $$$$  /$$$$ |");
            Console.WriteLine(@"        $$ |      $$ |$$$$/     $$    $$ | $$$$$$  |  $$ |  $$ $$ $$/$$ |");
            Console.WriteLine(@"        $$ |_____ $$ |$$ |      $$$$$$$$/ /  \__$$ | _$$ |_ $$ |$$$/ $$ |");
            Console.WriteLine(@"        $$       |$$ |$$ |      $$       |$$    $$/ / $$   |$$ | $/  $$ |");
            Console.WriteLine(@"        $$$$$$$$/ $$/ $$/        $$$$$$$/  $$$$$$/  $$$$$$/ $$/      $$/ ");
            Console.SetCursorPosition(25, 12);
            bool val = false;
            if (File.Exists(savePath))
            {
                StreamReader sr = new StreamReader(savePath);
                val = bool.Parse(sr.ReadLine()); // just read one line
                sr.Close();
                if (val) // is there saved data ?
                {
                    Console.Write("Would you like continue saved simulation ?");
                    Console.SetCursorPosition(25, 13);
                    Console.Write("(Yes-'Y' / No-'N') : "); // ask to user
                }
                // there is not saved file, create empty board
                else Console.Write("    Welcome to the LifeSim \n                   Press any key to contunie to simulation...");
            }
            // save file does not found, create empty board
            else { Console.Write("  Welcome to the LifeSim \n                   Press any key to contunie to simulation..."); ClearScreen(); }
            if (val) // is there saved data ?
            {
                while (true)
                {
                    string command = Console.ReadLine().ToLower();
                    if (command == "y") // get saved data
                    {
                        if (File.Exists(savePath)) GetData();
                        break;
                    }
                    else if (command == "n") { ClearScreen(); break; } // create empty board
                    else { Console.SetCursorPosition(37, 14); Console.Write("Incorrect value"); Console.SetCursorPosition(46, 13); Console.Write("  "); Console.SetCursorPosition(46, 13); }
                }
            }
            else { Console.ReadKey(); ClearScreen(); } // there is not saved data
            Console.Clear();
        }
        private static void SaveSimulation() // simulation datas saves to txt file
        {
            if (File.Exists(savePath))
            {
                FileStream fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(save.ToString()); // is there saved file ?
                for (byte a = 0; a < 16; a++)
                    for (byte b = 0; b < 32; b++) sw.WriteLine(board[a, b]); // board data
                sw.WriteLine(step); // step data
                sw.Flush();
                sw.Close();
                fs.Close();
            }
            else { StreamWriter f = File.CreateText(savePath); f.Close(); }
        }
        private static void GetData() // get data that from saved file
        {
            if (File.Exists(savePath))
            {
                StreamReader sr = new StreamReader(savePath);
                byte rows = 0, cols = 0; bool first = false;
                while (!sr.EndOfStream)
                {
                    string read = sr.ReadLine();
                    if (first && read != "")
                    {
                        if (rows > 15) { step = byte.Parse(read); } // get step
                        else { board[rows, cols] = Convert.ToChar(read); } // get board data
                        cols++;
                    }
                    first = true;
                    if (cols > 31) { cols = 0; rows++; }
                }
                sr.Close();
            }

        }
        private static void MakeColour(char color)
        {
            switch (color)
            {
                case 'r': Console.ForegroundColor = ConsoleColor.Red; break;
                case 'g': Console.ForegroundColor = ConsoleColor.Green; break;
                case 'w': Console.ForegroundColor = ConsoleColor.White; break;
                default: Console.ForegroundColor = (ConsoleColor)Random(0,20); break; // get random colour
            }
        }
        private static void KeyControl()
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true); // Get a key from user
                    save = true; // contunies to save
                    switch (key.Key)
                    {
                        case ConsoleKey.Q: Locate('q'); break;  // Locate Particle-Q
                        case ConsoleKey.W: Locate('w'); break; // Locate Particle-W
                        case ConsoleKey.E: Locate('e'); break; // Locate Particle-E
                        case ConsoleKey.R: Locate('r'); break; // Locate Particle-R
                        case ConsoleKey.T: CreateParticle('r'); break; // Locate Particle-T
                        case ConsoleKey.Y: CreateParticle('r'); Locate(); break; // Create new random Particle and locate it
                        case ConsoleKey.Spacebar: GoNewStep(); break; // go next step
                        case ConsoleKey.NumPad0: case ConsoleKey.D0: ClearScreen(); break; // clear simulation screen
                        case ConsoleKey.NumPad1: case ConsoleKey.D1: Rotate(0); break;// particle rotation 1 to Particle-Q 
                        case ConsoleKey.NumPad2: case ConsoleKey.D2: Rotate(1); break; // particle rotation 2 to Particle-W
                        case ConsoleKey.NumPad3: case ConsoleKey.D3: DeleteCell(); break;// delete a cell on the cursor
                        case ConsoleKey.NumPad4: case ConsoleKey.D4: Rotate(3); break; // particle rotation 3 to Particle-E
                        case ConsoleKey.UpArrow: ChangeLocation("up"); break; // Direction KEYS
                        case ConsoleKey.DownArrow: ChangeLocation("down"); break;
                        case ConsoleKey.RightArrow: ChangeLocation("right"); break;
                        case ConsoleKey.LeftArrow: ChangeLocation("left"); break;
                        case ConsoleKey.Enter: ContinueStep(); break; // until press enter again, screen is refreshed
                        default: error = true; break;
                    }
                    SaveSimulation();  // save simulation data
                    DravScreen(); // update simulation screen
                }
            }
        }
        private static void ClearScreen()
        { // simulation may be reseted using this func
            for (byte i = 0; i < 16; i++)
                for (byte j = 0; j < 32; j++) { board[i, j] = dead; }
            step = 0; cursorX = 29; cursorY = 7;
            save = false; // for empty saved file 
            SaveSimulation(); // clear board and save clean board to txt file
        }
        private static void DravScreen()
        {
            Console.Clear(); // screen is cleared
            Console.CursorVisible = false; // bug fix : Screen fluctuation was prevented
            MakeColour('w');
            int row = 1, cols = 1;
            if (debug) // Debug mode 
            {
                Console.SetCursorPosition(0, 18);
                Console.WriteLine("-------------------------- DEBUG MODE ---------------------------");
                    Console.Write("   Cursor: {0},{1}  |  Board: {2},{3}", cursorX,cursorY, BoardIndex('x'), BoardIndex('y'));
                Console.Write("          -----Neighbors------"); Console.SetCursorPosition(30, 20);
                Console.Write("            Live : {0} {1} : Dead", CountNeighbours(BoardIndex('x'), BoardIndex('y'), "live"), CountNeighbours(BoardIndex('x'), BoardIndex('y'), "dead"));
                Console.SetCursorPosition(0, 20);
                for (byte u = 0; u < 4; u++)
                {
                    Console.Write("Particle - {0}  : ", u + 1);
                    for (byte i = 0; i < 9; i++) Console.Write(particles[u, i] + " ");
                    Console.WriteLine();
                }
                Console.WriteLine("Total number of live to die : {0}", debugDeadCount);
                Console.WriteLine("Total number of die to live : {0}", debugLiveCount);
            }
            Console.SetCursorPosition(0, 0);
            for (byte i = 0; i < 16; i++) // Screen is printed using some ascii codes
            {
                if (i == 0) Console.Write("┌");
                else if (i == 15) { Console.SetCursorPosition(0, 17); Console.Write("└"); }
                Console.SetCursorPosition(0, row);
                Console.Write("│");
                for (byte j = 0; j < 32; j++)
                {
                    Console.SetCursorPosition(j + 1, 0);
                    if (i == 1) Console.SetCursorPosition(j+1 + 30, 0);
                    if (i == 14) Console.SetCursorPosition(j + 1, 17);
                    else if (i == 15) Console.SetCursorPosition(j + 1 + 30, 17);
                    if (i == 0 || i == 1 || i == 14 || i == 15) { Console.Write("──"); }
                    Console.SetCursorPosition(cols, row);
                    // Simulation board is writed on screen
                    if (board[i, j] == live) MakeColour('g'); else MakeColour('r');
                    Console.Write(board[i, j]);
                    MakeColour('w');
                    cols +=2;
                }
                if (i == 1) { Console.SetCursorPosition(64, 0); Console.WriteLine("┐"); }
                Console.SetCursorPosition(64, row);
                Console.Write("│");
                row++; cols = 1;
                Console.SetCursorPosition(cols, row);
                if (i == 15) { Console.SetCursorPosition(64, 17); Console.WriteLine("┘"); }
            }
            Console.SetCursorPosition(68, 1);
            Console.Write("Step : {0}   Live : {1}", step, CountLive());
            Console.SetCursorPosition(68, 4);
            Console.Write("Q");
            Console.SetCursorPosition(68, 8);
            Console.Write("W");
            Console.SetCursorPosition(68, 12);
            Console.Write("R");
            int rows = 3; cols = 73;
            // particles are printed
            for (byte u = 0; u < 4; u++)
            {
                if (u == 2) continue; // particle R, do not write particle-E
                for (byte i = 0; i < 9; i++)
                {
                    Console.SetCursorPosition(cols, rows);
                    Console.Write(particles[u, i] + " ");
                    cols += 2;
                    if (i == 2 || i == 5) { rows++; cols = 73; }
                }
                cols = 73; rows += 2;
            }
            if (cursorX > 63) cursorX = 63; // bug fix : out of board
            Console.SetCursorPosition(68, 16);  if (error) { Console.Write("Invalid KEY !"); error = false; }
            Console.SetCursorPosition(cursorX, cursorY);
            Console.CursorVisible = true; // bug fix : Screen fluctuation was prevented
        }
        private static int BoardIndex(char direction)
        {  // cursor location transfers to board[indexs],due to some process
            if(direction == 'x') return cursorY - 1;
            else return cursorX / 2;
        }
        private static int Random(int s1, int s2)
        {
            Random RD = new Random();
            return RD.Next(s1, s2);
        }
        private static void CreateParticle(char particle)
        {
            int num = 0, part = 0; // part is a index that particle
            switch(particle)
            {
                case 'q': part = 0; break; case 'w': part = 1; break;
                case 'e': part = 2; break; case 'r': num = Random(4, 7); part = 3; break;
            }
            for (byte i = 0; i < 9; i++) particles[part, i] = dead; // Firstly, particle's all index is made dead
            if(particle != 'r') // Particles Q-W-E is static
            {
                particles[0, 1] = live; particles[0, 4] = live; particles[0, 7] = live; // particle Q
                particles[1, 2] = live; particles[1, 3] = live; particles[1, 5] = live; particles[1, 7] = live; particles[1, 8] = live; // particle W
                particles[2, 4] = live; // particle E
            }
            byte numCounter = 0;
            while (true && part == 3) // partcle R is dynamic Random between 4 and 6
            {
                if (numCounter == num) { break; }
                int d = Random(0, 9);
                if (particles[part, d] == dead)
                {
                    numCounter++;
                    particles[part, d] = live;
                }
            }
        }
        private static void Locate(char particle)
        {
            int tempX = cursorX, tempY = cursorY; // center of choosen particle
            int chose = 0; // particle's index
            switch (particle)
            {
                case 'q': chose = 0; break; case 'w': chose = 1; break;
                case 'e': chose = 2; break; case 'r': chose = 3; break;
            }
            bool left = false, right = false;

            for (byte u = 1; u < 17; u++) if (cursorX == 1 && cursorY == u) left = true;    // cursor at left side
            for (byte u = 1; u < 17; u++) if (cursorX == 63 && cursorY == u) right = true;  //           right side

            if (left == false && right == false && cursorY < 16) // locate normal
            {
                cursorX -= 2; cursorY -= 1;// Center of particles
                for (byte i = 0; i < 9; i++)
                {
                    if (cursorY > 0)
                    {
                        if (cursorX < 0) cursorX = 1;
                        Console.SetCursorPosition(cursorX, cursorY);
                        if (board[BoardIndex('x'), BoardIndex('y')] == dead) board[BoardIndex('x'), BoardIndex('y')] = particles[chose, i];
                        cursorX += 2;
                        // if selected cell is live, do not modify it.
                    }
                    if (i == 2 || i == 5) { cursorY++; ; cursorX = tempX - 2; }
                }
                if (cursorX > 63) cursorX = 63;
            }
            else if (left == true) // locate left
            {
                // Is Cursor at corner ?
                if (cursorX == 1 && cursorY == 1) // just draw 4 cels
                {
                    //   Cursor : 1.1   |  Indexs :   4    5    6    7
                    //                               0.0  0.1  1.0  1.1
                    CornerControl(0, 0, chose, 4, 8); cursorX += 2; cursorY++;
                }
                else if(cursorX == 1 && cursorY == 16) // just draw 4 cels
                {
                    //   Cursor : 1.16   |  Indexs :   1     2     4     5
                    //                               14.0  14.1  15.0  15.1
                    CornerControl(14, 0, chose, 1, 5);
                }
                else // draw 6 cels
                {
                    byte[] particleIndex = { 1, 2, 4, 5, 7, 8 };
                    CornerControl(chose, particleIndex,"left");
                }
            }
            else if(right) // locate right
            {
                if (cursorX == 63 && cursorY == 1) // just drive 4 cels
                {
                    //   Cursor : 63.1   |  Indexs :   3     4     6     7
                    //                                0.30  0.31  1.30  1.31
                    CornerControl(0, 30, chose, 3, 7);
                }
                else if (cursorX == 63 && cursorY == 16) // just drive 4 cels
                {
                    //   Cursor : 63.16   |  Indexs :    0      1      3      4
                    //                               14.30  14.31  15.30  15.31
                    CornerControl(14, 30, chose, 0, 4);
                }
                else
                {
                    byte[] particleIndex = { 0, 1, 3, 4, 6, 7 };
                    CornerControl(chose, particleIndex,"right");
                }
            } // locate down
            else
            {
                CornerControl(chose, null, "down");
            }
        }
        private static void Locate()
        { // just located Random particle-R
            Random rd = new Random();
            int X = rd.Next(2, 14);
            int Y = rd.Next(2, 30); //center of particle
            X--; Y--; int temp = Y; // start writing
            for (byte u = 0; u < 9; u++)
            {
                if(board[X,Y] == dead) board[X, Y] = particles[3, u];
                Y++;
                if(u == 2 || u == 5) { X++; Y = temp; }
            }
        }
        private static void CornerControl(int X, int Y, int Particle, int firstID, int LastID)
        { // deep points control (1,1) (1,16) (63,1) (63,16)
            int c1 = X, c2 = Y;        // CURSOR (X,Y), Particle Num, Locating first index, and last index
            for (int u = firstID; u < LastID+1; u++)
            {
                if (board[c1, c2] == dead) board[c1, c2] = particles[Particle, u];
                if (u == LastID-3) u++;
                c2++;
                if (c2 == Y+2) { c2 = Y; c1++; }
            }
        }
        private static void CornerControl(int particle, byte[] particleIndex, string direction)
        {   // right, left and down edge control
            int X = BoardIndex('x'); // According to current cursor's location
            int Y = BoardIndex('y');
            int tempY = Y;
            if (direction == "down")
            {
                X--; Y -= 2;
                for (byte u = 0; u < 6; u++) // change 6 cells
                {
                    if (board[X, Y + 1] == dead) board[X, Y + 1] = particles[particle, u];
                    Y++;
                    if(u == 2) { Y = tempY - 2; X++; }
                }
            }
            else
            {
                if (direction == "right") { Y--; tempY = Y; } // bug fix : out of board index is bigger than 31
                for (byte u = 0; u < 6; u++) // change 6 cells
                {
                    if (board[X - 1, Y] == dead) board[X - 1, Y] = particles[particle, particleIndex[u]];
                    Y++;
                    if (Y % 2 == 0 && u != 0) { Y = tempY; X++; }
                }
            }
        }
        private static void ContinueStep()
        {
            while (true)
            {
                if(!Console.KeyAvailable)
                {
                    GoNewStep(); SaveSimulation(); DravScreen(); // refresh board
                    System.Threading.Thread.Sleep(111); // wait time
                }
                else { ConsoleKeyInfo k = Console.ReadKey(true); if(k.Key == ConsoleKey.Enter) break; } // until press enter again
            }
        }
        private static void GoNewStep()
        {
            step++; /* increase step */ cursorX = 29; cursorY = 8; // center of board
            CountNeighbours(0, 0, "live", true);// update neighbours
            for (byte u = 0; u < 16; u++)
            {
                for (byte k = 0; k < 32; k++) // each cell is controled by ordered
                {   // control evolation
                    char current = board[u, k];
                    int liveNeig = neigbours[u, k];
                    if (current == dead && liveNeig == 3) { current = live; debugLiveCount++; } // become live
                    else if (current == live && (liveNeig == 2 || liveNeig == 3)) { /* nothing */ } // lives next generation
                    else if (current == live && liveNeig < 2) { current = dead; debugDeadCount++; } // become dead due to underpopolation
                    else if (current == live && liveNeig > 3) { current = dead; debugDeadCount++; } // become dead due to overpopulation
                    board[u, k] = current;
                }
            }
        }
        private static void Rotate(int particleNum)
        {
            char[] temp = new char[9]; // 90 degres clockwisie
            byte[] nums = { 6, 3, 0, 7, 4, 1, 8, 5, 2 }; // After rotation new index values
            for (byte u = 0; u < 9; u++) temp[u] = particles[particleNum, nums[u]]; // datas transfers temp array
            for (byte i = 0; i < 9; i++) particles[particleNum, i] = temp[i]; // temp transfers particle array
        }
        private static void ChangeLocation(string direction)
        {
            switch (direction)
            {   // cursor movementation
                case "up":    if (cursorY > 1) cursorY--;     break; case "down":  if (cursorY < 16) cursorY++;    break;
                case "right": if (cursorX < 63) cursorX += 2; break; case "left":  if (cursorX > 1) cursorX -= 2;  break;
            }
            Console.SetCursorPosition(cursorX, cursorY);
        }
        private static void DeleteCell()
        {
            board[BoardIndex('x'), BoardIndex('y')] = dead; // choosen cell will be dead
        }
        private static int CountLive()
        {
            int count = 0;
            for(byte i = 0; i < 16; i++)
                for(byte j = 0; j < 32; j++) if (board[i, j] == live) count++; // each cell is controled, counting live cells
            return count;
        }
        private static int CountNeighbours(int X, int Y, string kind, bool all = false)
        {
            if (all) // count live neighbours,and transfer to neighbour array
            {
                for (byte a = 0; a < 16; a++) for (byte b = 0; b < 32; b++) neigbours[a, b] = CountNeighbours(a, b, "live"); // recursive func.
            }
            byte dimension1 = 2, dimension2 = 2; int liveNum = 0, deadNum = 0;
            int[] visit1, visit2;
            if (X == 0 && Y == 0) { visit1 = new int [] { 0, 1 }; visit2 = new int [] { 0, 1 }; }// corner 1 top left
            else if (X == 15 && Y == 0) { visit1 = new int [] { 14, 15 }; visit2 = new int [] { 0, 1 }; } // corner 2 bottom left
            else if (X == 0 && Y == 31) { visit1 = new int [] { 0, 1 }; visit2 = new int [] { 30, 31 }; } // corner 3 top right
            else if (X == 15 && Y == 31) { visit1 = new int [] { 14, 15 }; visit2 = new int [] { 30, 31 }; } // corner 4 bottom right
            else if (X >= 1 && X <= 14 && Y == 0) { visit1 = new int [] { X - 1, X, X+1 }; visit2 = new int [] { 0, 1 }; dimension1 = 3; } // left edge
            else if (X >= 1 && X <= 14 && Y == 31) { visit1 = new int [] { X - 1, X, X + 1 }; visit2 = new int [] { 30, 31 }; dimension1 = 3; } // right edge
            else if (X == 0 && Y >= 1 && Y <= 30) { visit1 = new int [] { 0 , 1 }; visit2 = new int [] { Y - 1, Y, Y + 1 }; dimension2 = 3; } // top edge
            else if (X == 15 && Y >= 1 && Y <= 30) { visit1 = new int [] { 14, 15 }; visit2 = new int [] { Y - 1, Y, Y + 1 }; dimension2 = 3; } // bottom edge
            else { visit1 = new int [] { X - 1, X, X + 1}; visit2 = new int [] { Y - 1, Y, Y + 1 }; dimension1 = 3; dimension2 = 3; } // normal
            for (byte u = 0; u < dimension1; u++)
                for (byte k = 0; k < dimension2; k++) if (board[visit1[u], visit2[k]] == live) liveNum++; else deadNum++;
            if (board[X, Y] == live) liveNum--; else deadNum--;
            if (kind == "live") return liveNum;
            else return deadNum;
        }
    }
}