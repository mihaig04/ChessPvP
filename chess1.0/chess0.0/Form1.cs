using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media;
using System.Diagnostics;

namespace chess0._0
{
    public partial class Form1 : Form
    {
        Button new_game;
        Panel my_panel;
        Label copyright, turn;
        ListBox promotion_choice;
        SoundPlayer sound;

        public Form1()
        {
            InitializeComponent();
            this.Icon = new Icon("../../Resources/icon1.ico");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = new Size(900, 740);
            this.Text = "Chess PvP";
            this.BackColor = Color.Black;

            engine.table = new position[8, 8];
            engine.piece_collection = new piece[4, 8];
            engine.piece_selected = false;
            engine.position_of_piece_selected_i = -1;
            engine.position_of_piece_selected_j = -1;
            engine.black_turn = false;
            engine.en_passant = false;
            engine.en_passant_x = -1;
            engine.en_passant_y = -1;
            engine.promotion_x = -1;
            engine.promotion_y = -1;

            my_panel = new Panel();
            my_panel.Location = new Point(50, 50);
            my_panel.Size = new Size(600, 600);
            my_panel.BackColor = Color.LightPink;
            my_panel.Parent = this;
            my_panel.Anchor = AnchorStyles.None;

            new_game = new Button();
            new_game.Location = new Point(720, 50);
            new_game.Size = new Size(100, 30);
            //new_game.BackgroundImage = Image.FromFile(@"..\..\Resources\new_game.png");
            new_game.Text = "NEW GAME";
            new_game.BackColor = Color.Black;
            new_game.ForeColor = Color.White;
            new_game.TextAlign = ContentAlignment.MiddleCenter;
            //new_game.BackgroundImageLayout = ImageLayout.Stretch;
            new_game.Parent = this;
            new_game.Click += new_game_Click;
            new_game.Anchor = AnchorStyles.None;

            copyright = new Label();
            copyright.Location = new Point(710, 620);
            copyright.Size = new Size(300, 45);
            copyright.Text = "-------(c)2023--------\nMihai Ghergheles";
            copyright.ForeColor = Color.White;
            copyright.BackColor = Color.Transparent;
            copyright.Parent = this;
            copyright.Anchor = AnchorStyles.None;

            turn = new Label();
            turn.Location = new Point(720, 100);
            turn.Size = new Size(100, 30);
            turn.Text = "";
            turn.ForeColor = Color.White;
            turn.BackColor = Color.Black;
            turn.Parent = this;
            turn.Anchor = AnchorStyles.None;
            turn.TextAlign = ContentAlignment.MiddleCenter;
            turn.Text = "WHITE'S TURN";

            promotion_choice = new ListBox();
            promotion_choice.Location = new Point(720, 150);
            promotion_choice.Size = new Size(100, 60);
            promotion_choice.Parent = this;
            promotion_choice.SelectionMode = SelectionMode.One;
            promotion_choice.SelectedIndexChanged += new EventHandler(promotion_choice_SelectedIndexChanged);
            promotion_choice.Anchor = AnchorStyles.None;
            promotion_choice.Enabled = false;
            promotion_choice.Visible = false;
            
            //GAME START LAYOUT CONFIGURATION
            int spacing = 10, position_size = (my_panel.Width - spacing * 7) / 8;
            for (int i = 0; i < 8; ++i)
            {
                //TABLE INITIALIZATION
                for (int j = 0; j < 8; ++j)
                {
                    engine.table[i, j] = new position();
                    engine.table[i, j].piece_i = -1;
                    engine.table[i, j].piece_j = -1;
                    engine.table[i, j].x = i;
                    engine.table[i, j].y = j;

                    engine.table[i, j].Location = new Point(j * (spacing + position_size), i * (spacing + position_size));
                    engine.table[i, j].Size = new Size(position_size, position_size);

                    engine.table[i, j].Parent = my_panel;
                    engine.table[i, j].Click += Form1_Click;

                    engine.table[i, j].occupied = false;

                    engine.table[i, j].BackgroundImageLayout = ImageLayout.Stretch;

                    if ((i + j) % 2 == 0)
                        engine.table[i, j].BackColor = Color.AntiqueWhite;
                    else
                        engine.table[i, j].BackColor = Color.RoyalBlue;
                }
            }

            initialize_piece_collection_images();

            //ASSOCIATION BETWEEN TABLE AND PIECE COLLECTION
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 8; ++j)
                {
                    if (engine.piece_collection[i, j].active)
                    {
                        engine.table[engine.piece_collection[i, j].x, engine.piece_collection[i, j].y].BackgroundImage = Image.FromFile(@"..\..\Resources\" + engine.piece_collection[i, j].this_color + "_" + engine.piece_collection[i, j].this_role + ".png");
                        engine.table[engine.piece_collection[i, j].x, engine.piece_collection[i, j].y].occupied = true;
                        engine.table[engine.piece_collection[i, j].x, engine.piece_collection[i, j].y].piece_i = i;
                        engine.table[engine.piece_collection[i, j].x, engine.piece_collection[i, j].y].piece_j = j;
                    }
                }

            }
        }

        //MENU FOR PROMOTION OF PAWN OR FOR GAME END
        private void promotion_choice_SelectedIndexChanged(object sender, EventArgs e)
        {
            string promotion_role = promotion_choice.SelectedItem.ToString();

            if (promotion_role == "game draw")
            {
                sound = new SoundPlayer(@"..\..\Resources\notify.wav");
                sound.Play();
                MessageBox.Show("DRAW", "NO WINNER");

                Form1 x = new Form1();
                this.Hide();
                x.ShowDialog();
                this.Close();
            }
            else if (promotion_role == "resignation")
            {
                sound = new SoundPlayer(@"..\..\Resources\notify.wav");
                sound.Play();
                if (engine.black_turn)
                    MessageBox.Show("GAME RESIGNED BY BLACK", "WHITE WINS");
                else
                    MessageBox.Show("GAME RESIGNED BY WHITE", "BLACK WINS");

                Form1 x = new Form1();
                this.Hide();
                x.ShowDialog();
                this.Close();
            }
            else
            {
                int piece_i = engine.table[engine.promotion_x, engine.promotion_y].piece_i,
                    piece_j = engine.table[engine.promotion_x, engine.promotion_y].piece_j;

                engine.piece_collection[piece_i, piece_j].this_role = promotion_role;
                engine.table[engine.promotion_x, engine.promotion_y].BackgroundImage = Image.FromFile(@"..\..\Resources\" + engine.piece_collection[piece_i, piece_j].this_color + "_" + engine.piece_collection[piece_i, piece_j].this_role + ".png");

                engine.black_turn = !engine.black_turn;

                string piece_color;
                if (engine.black_turn)
                {
                    piece_color = "black";
                }
                else
                {
                    piece_color = "white";
                }
                turn.Text = piece_color.ToUpper() + "'S TURN";

                flip_board();

                engine.piece_collection[engine.table[engine.promotion_x, engine.promotion_y].piece_i, engine.table[engine.promotion_x, engine.promotion_y].piece_j].has_been_moved = true;

                promotion_choice.Items.Clear();
                promotion_choice.Enabled = false;
                promotion_choice.Visible = false;
            }
        }
        
        private void new_game_Click(object sender, EventArgs e)
        {
            if (promotion_choice.Items.Count == 0)
            {
                promotion_choice.Enabled = true;
                promotion_choice.Visible = true;
                promotion_choice.Items.Clear();
                promotion_choice.Items.Add("resignation");
                promotion_choice.Items.Add("game draw");
            }
            else if (promotion_choice.Items.Count == 2)
            {
                promotion_choice.Enabled = false;
                promotion_choice.Visible = false;
                promotion_choice.Items.Clear();
            }
            else
            {
                sound = new SoundPlayer(@"..\..\Resources\notify.wav");
                sound.Play();
                MessageBox.Show("COMPLETE THE MOVE", "CANNOT END GAME NOW");
            }
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            position position_clicked = (sender as position);

            string piece_color;
            if (engine.black_turn)
                piece_color = "black";
            else
                piece_color = "white";

            if (engine.piece_selected)
            {
                //SIMULTANEOUS DESELECTION AND SELECTION OF NEW PIECE
                if (position_clicked.occupied && engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_color == piece_color)
                {
                    //CASTLE
                    if (engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_role == "king"
                        && engine.piece_collection[engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].piece_i, engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].piece_j].this_role == "rook"
                        && !engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].has_been_moved
                        && !engine.piece_collection[engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].piece_i, engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].piece_j].has_been_moved)
                    {
                        int old_position_i = engine.position_of_piece_selected_i,
                                old_position_j = engine.position_of_piece_selected_j,
                                old_piece_i = engine.table[old_position_i, old_position_j].piece_i,
                                old_piece_j = engine.table[old_position_i, old_position_j].piece_j,
                                new_piece_i = position_clicked.piece_i,
                                new_piece_j = position_clicked.piece_j;

                        int multiplier = -1, new_position_rook_y = 3; // LEFT ROOK
                        if (engine.position_of_piece_selected_j > position_clicked.y) //RIGHT ROOK
                        {
                            multiplier = 1;
                            new_position_rook_y = 5;
                        }

                        if (move_is_possible(engine.position_of_piece_selected_i, new_position_rook_y, piece_color))
                        {
                            position_clicked.piece_i = -1;
                            position_clicked.piece_j = -1;
                            position_clicked.BackgroundImage = null;
                            position_clicked.occupied = false;

                            engine.table[old_position_i, old_position_j].BackgroundImage = null;
                            engine.table[old_position_i, old_position_j].occupied = false;
                            engine.table[old_position_i, old_position_j].piece_i = -1;
                            engine.table[old_position_i, old_position_j].piece_j = -1;

                            engine.piece_collection[old_piece_i, old_piece_j].x = old_position_i;
                            engine.piece_collection[old_piece_i, old_piece_j].y = new_position_rook_y;

                            engine.table[old_position_i, new_position_rook_y].BackgroundImage = Image.FromFile(@"..\..\Resources\" + engine.piece_collection[old_piece_i, old_piece_j].this_color + "_" + engine.piece_collection[old_piece_i, old_piece_j].this_role + ".png");
                            //ROOK IS NOT ADDED YET TO ALLOW "king_is_safe" CHECK WHILE CASTLING
                            //engine.table[old_position_i, new_position_rook_y].occupied = true;
                            //engine.table[old_position_i, new_position_rook_y].piece_i = old_piece_i;
                            //engine.table[old_position_i, new_position_rook_y].piece_j = old_piece_j;

                            engine.piece_selected = false;
                            engine.position_of_piece_selected_i = -1;
                            engine.position_of_piece_selected_j = -1;

                            engine.piece_collection[new_piece_i, new_piece_j].x = position_clicked.x;
                            engine.piece_collection[new_piece_i, new_piece_j].y = position_clicked.y + 2 * multiplier;

                            engine.table[position_clicked.x, position_clicked.y + 2 * multiplier].BackgroundImage = Image.FromFile(@"..\..\Resources\" + engine.piece_collection[new_piece_i, new_piece_j].this_color + "_" + engine.piece_collection[new_piece_i, new_piece_j].this_role + ".png");
                            //KING IS NOT ADDED YET TO ALLOW "king_is_safe" CHECK WHILE CASTLING
                            //engine.table[position_clicked.x, position_clicked.y + 2 * multiplier].occupied = true;
                            //engine.table[position_clicked.x, position_clicked.y + 2 * multiplier].piece_i = new_piece_i;
                            //engine.table[position_clicked.x, position_clicked.y + 2 * multiplier].piece_j = new_piece_j;
                                                            
                            if (!king_is_safe(piece_color, "new possible position specified", position_clicked.x, position_clicked.y)
                                || !king_is_safe(piece_color, "new possible position specified", position_clicked.x, position_clicked.y + multiplier)
                                || !king_is_safe(piece_color, "new possible position specified", position_clicked.x, position_clicked.y + 2 * multiplier))
                            {
                                //REVERSION MOVE
                                sound = new SoundPlayer(@"..\..\Resources\notify.wav");
                                sound.Play();
                                MessageBox.Show("Try a different move if possible.", "King of " + piece_color + " is checked.");

                                position_clicked.piece_i = new_piece_i;
                                position_clicked.piece_j = new_piece_j;
                                position_clicked.BackgroundImage = Image.FromFile(@"..\..\Resources\" + engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_color + "_" + engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_role + ".png");
                                position_clicked.occupied = true;

                                engine.table[old_position_i, old_position_j].BackgroundImage = Image.FromFile(@"..\..\Resources\" + engine.piece_collection[old_piece_i, old_piece_j].this_color + "_" + engine.piece_collection[old_piece_i, old_piece_j].this_role + ".png");
                                engine.table[old_position_i, old_position_j].occupied = true;
                                engine.table[old_position_i, old_position_j].piece_i = old_piece_i;
                                engine.table[old_position_i, old_position_j].piece_j = old_piece_j;

                                engine.piece_collection[old_piece_i, old_piece_j].x = old_position_i;
                                engine.piece_collection[old_piece_i, old_piece_j].y = old_position_j;

                                engine.piece_selected = true;
                                engine.position_of_piece_selected_i = old_position_i;
                                engine.position_of_piece_selected_j = old_position_j;

                                engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].x = position_clicked.x;
                                engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].y = position_clicked.y;
                                engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].active = true;

                                //engine.table[position_clicked.x, position_clicked.y + 2 * multiplier].occupied = false;
                                engine.table[position_clicked.x, position_clicked.y + 2 * multiplier].BackgroundImage = null;
                                //engine.table[position_clicked.x, position_clicked.y + 2 * multiplier].piece_i = -1;
                                //engine.table[position_clicked.x, position_clicked.y + 2 * multiplier].piece_j = -1;

                                //engine.table[old_position_i, new_position_rook_y].occupied = false;
                                engine.table[old_position_i, new_position_rook_y].BackgroundImage = null;
                                //engine.table[old_position_i, new_position_rook_y].piece_i = -1;
                                //engine.table[old_position_i, new_position_rook_y].piece_j = -1;
                            }
                            else
                            {
                                sound = new SoundPlayer(@"..\..\Resources\move-self.wav");
                                sound.Play();

                                //ADDITION OF KING
                                engine.table[position_clicked.x, position_clicked.y + 2 * multiplier].occupied = true;
                                engine.table[position_clicked.x, position_clicked.y + 2 * multiplier].piece_i = new_piece_i;
                                engine.table[position_clicked.x, position_clicked.y + 2 * multiplier].piece_j = new_piece_j;

                                //ADDITION OF ROOK
                                engine.table[old_position_i, new_position_rook_y].occupied = true;
                                engine.table[old_position_i, new_position_rook_y].piece_i = old_piece_i;
                                engine.table[old_position_i, new_position_rook_y].piece_j = old_piece_j;

                                engine.black_turn = !engine.black_turn;

                                if (engine.black_turn)
                                {
                                    piece_color = "black";
                                }
                                else
                                {
                                    piece_color = "white";
                                }
                                turn.Text = piece_color.ToUpper() + "'S TURN";

                                flip_board();

                                engine.piece_collection[new_piece_i, new_piece_j].has_been_moved = true;
                                engine.piece_collection[old_piece_i, old_piece_j].has_been_moved = true;
                            }

                            //REPAIRING THE ALTERATION FROM CALL OF "king_is_safe" FUNCTION WITH SPECIAL COMMAND
                            int king_i;
                            if (!engine.black_turn)
                                king_i = 3;
                            else
                                king_i = 0;
                            engine.table[engine.piece_collection[king_i, 4].x, engine.piece_collection[king_i, 4].y].occupied = true;
                        }
                    }
                    else
                    {
                        engine.piece_selected = true;
                        engine.position_of_piece_selected_i = position_clicked.x;
                        engine.position_of_piece_selected_j = position_clicked.y;
                    }
                }
                else //MOVE OF A PIECE
                {
                    if (move_is_possible(position_clicked.x, position_clicked.y, piece_color))
                    {
                        if (position_clicked.occupied) // && piece_color != engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_color) //CHECK TO NOT BE ABLE TO REMOVE YOUR OWN PIECE WITH YOUR OWN PIECE - not needed
                        {
                            int old_position_i = engine.position_of_piece_selected_i,
                                old_position_j = engine.position_of_piece_selected_j,
                                old_piece_i = engine.table[old_position_i, old_position_j].piece_i,
                                old_piece_j = engine.table[old_position_i, old_position_j].piece_j,
                                new_piece_i = position_clicked.piece_i,
                                new_piece_j = position_clicked.piece_j;

                            //REMOVE OF THE OLD PIECE
                            engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].x = -1;
                            engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].y = -1;
                            engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].active = false;

                            //MOVE OF THE NEW PIECE
                            position_clicked.piece_i = engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].piece_i;
                            position_clicked.piece_j = engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].piece_j;
                            position_clicked.BackgroundImage = Image.FromFile(@"..\..\Resources\" + engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_color + "_" + engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_role + ".png");
                            position_clicked.occupied = true;

                            engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].BackgroundImage = null;
                            engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].occupied = false;
                            engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].piece_i = -1;
                            engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].piece_j = -1;

                            engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].x = position_clicked.x;
                            engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].y = position_clicked.y;

                            engine.piece_selected = false;
                            engine.position_of_piece_selected_i = -1;
                            engine.position_of_piece_selected_j = -1;
                            
                            if (!king_is_safe(piece_color, "", -1, -1))
                            {
                                //REVERSION MOVE
                                sound = new SoundPlayer(@"..\..\Resources\notify.wav");
                                sound.Play();
                                MessageBox.Show("Try a different move if possible.", "King of " + piece_color + " is checked.");

                                position_clicked.piece_i = new_piece_i;
                                position_clicked.piece_j = new_piece_j;
                                position_clicked.BackgroundImage = Image.FromFile(@"..\..\Resources\" + engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_color + "_" + engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_role + ".png");
                                position_clicked.occupied = true;

                                engine.table[old_position_i, old_position_j].BackgroundImage = Image.FromFile(@"..\..\Resources\" + engine.piece_collection[old_piece_i, old_piece_j].this_color + "_" + engine.piece_collection[old_piece_i, old_piece_j].this_role + ".png");
                                engine.table[old_position_i, old_position_j].occupied = true;
                                engine.table[old_position_i, old_position_j].piece_i = old_piece_i;
                                engine.table[old_position_i, old_position_j].piece_j = old_piece_j;

                                engine.piece_collection[old_piece_i, old_piece_j].x = old_position_i;
                                engine.piece_collection[old_piece_i, old_piece_j].y = old_position_j;

                                engine.piece_selected = true;
                                engine.position_of_piece_selected_i = old_position_i;
                                engine.position_of_piece_selected_j = old_position_j;

                                engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].x = position_clicked.x;
                                engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].y = position_clicked.y;
                                engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].active = true;
                            }
                            else
                            {
                                sound = new SoundPlayer(@"..\..\Resources\capture.wav");
                                sound.Play();

                                engine.en_passant = false;

                                //PROMOTION OF PAWN
                                if ( engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_role == "pawn"
                                    && ((!engine.black_turn && position_clicked.x == 0)
                                        || (engine.black_turn && position_clicked.x == 7)) )
                                {
                                    engine.promotion_x = position_clicked.x;
                                    engine.promotion_y = position_clicked.y;

                                    for (int i = 4; i > 0; --i)
                                        promotion_choice.Items.Add(engine.role[i]);
                                    promotion_choice.Enabled = true;
                                    promotion_choice.Visible = true;
                                }
                                else
                                {
                                    engine.black_turn = !engine.black_turn;

                                    if (engine.black_turn)
                                    {
                                        piece_color = "black";
                                    }
                                    else
                                    {
                                        piece_color = "white";
                                    }
                                    turn.Text = piece_color.ToUpper() + "'S TURN";

                                    flip_board();

                                    engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].has_been_moved = true;
                                }
                            }
                        }
                        else if (!position_clicked.occupied)
                        {
                            int old_position_i = engine.position_of_piece_selected_i,
                                old_position_j = engine.position_of_piece_selected_j,
                                old_piece_i = engine.table[old_position_i, old_position_j].piece_i,
                                old_piece_j = engine.table[old_position_i, old_position_j].piece_j,
                                new_piece_i = position_clicked.piece_i,
                                new_piece_j = position_clicked.piece_j;


                            //MOVE OF THE NEW PIECE
                            position_clicked.piece_i = engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].piece_i;
                            position_clicked.piece_j = engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].piece_j;
                            position_clicked.BackgroundImage = Image.FromFile(@"..\..\Resources\" + engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_color + "_" + engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_role + ".png");
                            position_clicked.occupied = true;

                            engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].BackgroundImage = null;
                            engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].occupied = false;
                            engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].piece_i = -1;
                            engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].piece_j = -1;

                            engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].x = position_clicked.x;
                            engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].y = position_clicked.y;

                            engine.piece_selected = false;
                            engine.position_of_piece_selected_i = -1;
                            engine.position_of_piece_selected_j = -1;

                            if (!king_is_safe(piece_color, "", -1, -1))
                            {
                                //REVERSION MOVE
                                sound = new SoundPlayer(@"..\..\Resources\notify.wav");
                                sound.Play();
                                MessageBox.Show("Try a different move if possible.", "King of " + piece_color + " is checked.");

                                position_clicked.piece_i = -1;
                                position_clicked.piece_j = -1;
                                position_clicked.BackgroundImage = null;
                                position_clicked.occupied = false;

                                engine.table[old_position_i, old_position_j].BackgroundImage = Image.FromFile(@"..\..\Resources\" + engine.piece_collection[old_piece_i, old_piece_j].this_color + "_" + engine.piece_collection[old_piece_i, old_piece_j].this_role + ".png");
                                engine.table[old_position_i, old_position_j].occupied = true;
                                engine.table[old_position_i, old_position_j].piece_i = old_piece_i;
                                engine.table[old_position_i, old_position_j].piece_j = old_piece_j;

                                engine.piece_collection[old_piece_i, old_piece_j].x = old_position_i;
                                engine.piece_collection[old_piece_i, old_piece_j].y = old_position_j;

                                engine.piece_selected = true;
                                engine.position_of_piece_selected_i = old_position_i;
                                engine.position_of_piece_selected_j = old_position_j;
                            }
                            else
                            {
                                //EN-PASSANT MOVE COMPLETION
                                if (engine.en_passant && engine.en_passant_y == position_clicked.y && engine.en_passant_x == old_position_i
                                    && engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_role == "pawn")
                                {
                                    engine.table[engine.en_passant_x, engine.en_passant_y].occupied = false;
                                    engine.table[engine.en_passant_x, engine.en_passant_y].BackgroundImage = null;
                                    engine.piece_collection[engine.table[engine.en_passant_x, engine.en_passant_y].piece_i, engine.table[engine.en_passant_x, engine.en_passant_y].piece_j].active = false;
                                    engine.piece_collection[engine.table[engine.en_passant_x, engine.en_passant_y].piece_i, engine.table[engine.en_passant_x, engine.en_passant_y].piece_j].x = -1;
                                    engine.piece_collection[engine.table[engine.en_passant_x, engine.en_passant_y].piece_i, engine.table[engine.en_passant_x, engine.en_passant_y].piece_j].y = -1;
                                    engine.table[engine.en_passant_x, engine.en_passant_y].piece_i = -1;
                                    engine.table[engine.en_passant_x, engine.en_passant_y].piece_j = -1;

                                    sound = new SoundPlayer(@"..\..\Resources\capture.wav");
                                    sound.Play();
                                }
                                else
                                {
                                    sound = new SoundPlayer(@"..\..\Resources\move-self.wav");
                                    sound.Play();
                                }
                                
                                //REMOVAL OF EN-PASSANT OPPORTUNITY
                                engine.en_passant = false;
                                engine.en_passant_x = -1;
                                engine.en_passant_y = -1;

                                //PROMOTION OF PAWN
                                if (engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_role == "pawn"
                                    && ((!engine.black_turn && position_clicked.x == 0)
                                        || (engine.black_turn && position_clicked.x == 7)))
                                {
                                    engine.promotion_x = position_clicked.x;
                                    engine.promotion_y = position_clicked.y;

                                    for (int i = 4; i > 0; --i)
                                        promotion_choice.Items.Add(engine.role[i]);
                                    promotion_choice.Enabled = true;
                                    promotion_choice.Visible = true;
                                }
                                else
                                {
                                    //EN-PASSANT OPPORTUNITY
                                    if (engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_role == "pawn"
                                         && !engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].has_been_moved
                                         && (position_clicked.x == 3 || position_clicked.x == 4))
                                    {
                                        engine.en_passant = true;
                                        engine.en_passant_x = position_clicked.x;
                                        engine.en_passant_y = position_clicked.y;
                                        //MessageBox.Show(engine.en_passant_x.ToString() + " " + engine.en_passant_y.ToString());
                                    }

                                    engine.black_turn = !engine.black_turn;

                                    if (engine.black_turn)
                                    {
                                        piece_color = "black";
                                    }
                                    else
                                    {
                                        piece_color = "white";
                                    }
                                    turn.Text = piece_color.ToUpper() + "'S TURN";

                                    flip_board();

                                    engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].has_been_moved = true;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                //SELECTION OF A PIECE
                if (position_clicked.occupied)
                {
                    if (engine.black_turn && engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_color == "black")
                    {
                        engine.piece_selected = true;
                        engine.position_of_piece_selected_i = position_clicked.x;
                        engine.position_of_piece_selected_j = position_clicked.y;
                    }
                    else if(!engine.black_turn && engine.piece_collection[position_clicked.piece_i, position_clicked.piece_j].this_color == "white")
                    {
                        engine.piece_selected = true;
                        engine.position_of_piece_selected_i = position_clicked.x;
                        engine.position_of_piece_selected_j = position_clicked.y;
                    }
                }
                
            }
        }

        //ESCAPE KEY TO EXIT APPLICATION
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (Form.ModifierKeys == Keys.None && keyData == Keys.Escape)
            {
                Menu x = new Menu();
                this.Hide();
                x.ShowDialog();
                this.Close();
            }
            if (Form.ModifierKeys == Keys.None && keyData == Keys.F)
            {
                engine.flip = !engine.flip;
                return true;
            }
            if (Form.ModifierKeys == Keys.None && keyData == Keys.F1)
            {
                Process.Start("https://en.wikipedia.org/wiki/Rules_of_chess");
            }
            return base.ProcessDialogKey(keyData);
        }

        //ROLE ASSIGNMENT
        public void initialize_piece_collection_images()
        {
            for (int j = 0; j < 8; ++j)
            {
                //PARTIAL INITIALIZATION ENTIRE PIECE COLLECTION
                for (int i = 0; i < 2; ++i)
                {
                    engine.piece_collection[i, j] = new piece();
                    engine.piece_collection[i, j].active = true;
                    engine.piece_collection[i, j].has_been_moved = false;
                    engine.piece_collection[i, j].x = i;
                    engine.piece_collection[i, j].y = j;
                    engine.piece_collection[i, j].this_color = "black";

                    engine.piece_collection[i + 2, j] = new piece();
                    engine.piece_collection[i + 2, j].active = true;
                    engine.piece_collection[i + 2, j].has_been_moved = false;
                    engine.piece_collection[i + 2, j].x = i + 6;
                    engine.piece_collection[i + 2, j].y = j;
                    engine.piece_collection[i + 2, j].this_color = "white";
                }

                //PAWNS
                engine.piece_collection[1, j].this_role = "pawn";

                engine.piece_collection[2, j].this_role = "pawn";
            }

            //ROOKS
            engine.piece_collection[0, 0].this_role = "rook";

            engine.piece_collection[0, 7].this_role = "rook";

            engine.piece_collection[3, 0].this_role = "rook";

            engine.piece_collection[3, 7].this_role = "rook";

            //KNIGHTS
            engine.piece_collection[0, 1].this_role = "knight";

            engine.piece_collection[0, 6].this_role = "knight";

            engine.piece_collection[3, 1].this_role = "knight";

            engine.piece_collection[3, 6].this_role = "knight";

            //BISHOPS
            engine.piece_collection[0, 2].this_role = "bishop";

            engine.piece_collection[0, 5].this_role = "bishop";

            engine.piece_collection[3, 2].this_role = "bishop";

            engine.piece_collection[3, 5].this_role = "bishop";

            //QUEENS
            engine.piece_collection[0, 3].this_role = "queen";

            engine.piece_collection[3, 3].this_role = "queen";

            //KINGS
            engine.piece_collection[0, 4].this_role = "king";

            engine.piece_collection[3, 4].this_role = "king";
        }

        public void flip_board()
        {
            if (engine.flip)
            {
                int spacing = 10, position_size = (my_panel.Width - spacing * 7) / 8;
                for (int i = 0; i < 8; ++i)
                {
                    for (int j = 0; j < 8; ++j)
                    {
                        if (engine.black_turn)
                            engine.table[i, j].Location = new Point((7 - j) * (spacing + position_size), (7 - i) * (spacing + position_size));
                        else
                            engine.table[i, j].Location = new Point(j * (spacing + position_size), i * (spacing + position_size));
                    }
                }
            }
        }

        //RETURNS FALSE IF KING IS CHECKED
        public bool king_is_safe(string king_color, string command, int new_possible_position_x, int new_possible_position_y)
        {
            int king_i;
            if(king_color == "white")
                king_i = 3;
            else
                king_i = 0;

            int king_x = engine.piece_collection[king_i, 4].x,
                king_y = engine.piece_collection[king_i, 4].y;

            if (command == "new possible position specified") // MODIFICATION NEEDED FOR CASTLE MOVE -> GO TO Form1_Click
            {
                king_x = new_possible_position_x;
                king_y = new_possible_position_y;
                engine.table[engine.piece_collection[king_i, 4].x, engine.piece_collection[king_i, 4].y].occupied = false;
            }

            //CHECK FROM:
            
            //ROOKS OR QUEEN

            //WEST
            for(int aux_y = king_y - 1; aux_y >= 0; --aux_y)
                if (engine.table[king_x, aux_y].occupied)
                {
                    if ((engine.piece_collection[engine.table[king_x, aux_y].piece_i, engine.table[king_x, aux_y].piece_j].this_role == "queen" || engine.piece_collection[engine.table[king_x, aux_y].piece_i, engine.table[king_x, aux_y].piece_j].this_role == "rook")
                        && engine.piece_collection[engine.table[king_x, aux_y].piece_i, engine.table[king_x, aux_y].piece_j].this_color != king_color)
                    {
                        //MessageBox.Show("rook/queen");
                        return false;
                    }
                    break;
                }
            //EAST
            for (int aux_y = king_y + 1; aux_y < 8; ++aux_y)
                if (engine.table[king_x, aux_y].occupied)
                {
                    if ((engine.piece_collection[engine.table[king_x, aux_y].piece_i, engine.table[king_x, aux_y].piece_j].this_role == "queen" || engine.piece_collection[engine.table[king_x, aux_y].piece_i, engine.table[king_x, aux_y].piece_j].this_role == "rook")
                        && engine.piece_collection[engine.table[king_x, aux_y].piece_i, engine.table[king_x, aux_y].piece_j].this_color != king_color)
                    {
                        //MessageBox.Show("rook/queen");
                        return false;
                    }
                    break;
                }
            //NORTH
            for (int aux_x = king_x - 1; aux_x >= 0; --aux_x)
                if (engine.table[aux_x, king_y].occupied)
                {
                    if ((engine.piece_collection[engine.table[aux_x, king_y].piece_i, engine.table[aux_x, king_y].piece_j].this_role == "queen" || engine.piece_collection[engine.table[aux_x, king_y].piece_i, engine.table[aux_x, king_y].piece_j].this_role == "rook")
                        && engine.piece_collection[engine.table[aux_x, king_y].piece_i, engine.table[aux_x, king_y].piece_j].this_color != king_color)
                    {
                        //MessageBox.Show("rook/queen");
                        return false;
                    }
                    break;
                }
            //SOUTH
            for (int aux_x = king_x + 1; aux_x < 8; ++aux_x)
                if (engine.table[aux_x, king_y].occupied)
                {
                    if ((engine.piece_collection[engine.table[aux_x, king_y].piece_i, engine.table[aux_x, king_y].piece_j].this_role == "queen" || engine.piece_collection[engine.table[aux_x, king_y].piece_i, engine.table[aux_x, king_y].piece_j].this_role == "rook")
                        && engine.piece_collection[engine.table[aux_x, king_y].piece_i, engine.table[aux_x, king_y].piece_j].this_color != king_color)
                    {
                        //MessageBox.Show("rook/queen");
                        return false;
                    }
                    break;
                }

            //BISHOPS OR QUEEN

            // "\" UP
            for(int aux_x = king_x - 1, aux_y = king_y - 1; aux_x >= 0 && aux_y >= 0; --aux_x, --aux_y)
                if (engine.table[aux_x, aux_y].occupied)
                {
                    if ((engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_role == "queen" || engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_role == "bishop")
                        && engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_color != king_color)
                    {
                        //MessageBox.Show("bishop/queen");
                        return false;
                    }
                    break;
                }
            // "\" DOWN
            for (int aux_x = king_x + 1, aux_y = king_y + 1; aux_x < 8 && aux_y < 8; ++aux_x, ++aux_y)
                if (engine.table[aux_x, aux_y].occupied)
                {
                    if ((engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_role == "queen" || engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_role == "bishop")
                        && engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_color != king_color)
                    {
                        //MessageBox.Show("bishop/queen");
                        return false;
                    }
                    break;
                }

            // "/" UP
            for (int aux_x = king_x - 1, aux_y = king_y + 1; aux_x >= 0 && aux_y < 8; --aux_x, ++aux_y)
                if (engine.table[aux_x, aux_y].occupied)
                {
                    if ((engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_role == "queen" || engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_role == "bishop")
                        && engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_color != king_color)
                    {
                        //MessageBox.Show("bishop/queen");
                        return false;
                    }
                    break;
                }

            // "/" DOWN
            for (int aux_x = king_x + 1, aux_y = king_y - 1; aux_x < 8 && aux_y >= 0; ++aux_x, --aux_y)
                if (engine.table[aux_x, aux_y].occupied)
                {
                    if ((engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_role == "queen" || engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_role == "bishop")
                        && engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_color != king_color)
                    {
                        //MessageBox.Show("bishop/queen");
                        return false;
                    }
                    break;
                }

            //KNIGHT
            for (int k = 0; k < 8; ++k)
            {
                int aux_x = engine.knight_dx[k] + king_x,
                    aux_y = engine.knight_dy[k] + king_y;
                if(aux_x >= 0 && aux_x < 8 && aux_y >= 0 && aux_y < 8)
                    if (engine.table[aux_x, aux_y].occupied
                        && engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_role == "knight"
                        && engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_color != king_color)
                    {
                        //MessageBox.Show("knight");
                        return false;
                    }
            }

            //PAWN
            if (king_color == "white")
            {
                if (king_x > 0 && (
                                     (king_y > 0 && engine.table[king_x - 1, king_y - 1].occupied
                                                 && engine.piece_collection[engine.table[king_x - 1, king_y - 1].piece_i, engine.table[king_x - 1, king_y - 1].piece_j].this_role == "pawn"
                                                 && engine.piece_collection[engine.table[king_x - 1, king_y - 1].piece_i, engine.table[king_x - 1, king_y - 1].piece_j].this_color != king_color)
                                  || (king_y < 7 && engine.table[king_x - 1, king_y + 1].occupied
                                                 && engine.piece_collection[engine.table[king_x - 1, king_y + 1].piece_i, engine.table[king_x - 1, king_y + 1].piece_j].this_role == "pawn"
                                                 && engine.piece_collection[engine.table[king_x - 1, king_y + 1].piece_i, engine.table[king_x - 1, king_y + 1].piece_j].this_color != king_color)
                                  )
                    )
                {
                    //MessageBox.Show("pawn");
                    return false;
                }
            }
            else
            {
                if (king_x < 7 && (
                                     (king_y > 0 && engine.table[king_x + 1, king_y - 1].occupied
                                                 && engine.piece_collection[engine.table[king_x + 1, king_y - 1].piece_i, engine.table[king_x + 1, king_y - 1].piece_j].this_role == "pawn"
                                                 && engine.piece_collection[engine.table[king_x + 1, king_y - 1].piece_i, engine.table[king_x + 1, king_y - 1].piece_j].this_color != king_color)
                                  || (king_y < 7 && engine.table[king_x + 1, king_y + 1].occupied
                                                 && engine.piece_collection[engine.table[king_x + 1, king_y + 1].piece_i, engine.table[king_x + 1, king_y + 1].piece_j].this_role == "pawn"
                                                 && engine.piece_collection[engine.table[king_x + 1, king_y + 1].piece_i, engine.table[king_x + 1, king_y + 1].piece_j].this_color != king_color)
                                  )
                    )
                {
                    //MessageBox.Show("pawn");
                    return false;
                }
            }

            //KING
            for (int k = 0; k < 8; ++k)
            {
                int aux_x = king_x + engine.king_dx[k],
                    aux_y = king_y + engine.king_dy[k];
                if (aux_x >= 0 && aux_x < 8 && aux_y >= 0 && aux_y < 8)
                    if (engine.table[aux_x, aux_y].occupied)
                        if (engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_role == "king"
                            && engine.piece_collection[engine.table[aux_x, aux_y].piece_i, engine.table[aux_x, aux_y].piece_j].this_color != king_color)
                        {
                            //MessageBox.Show("king");
                            return false;
                        }
            }

            return true;
        }

        //RETURNS TRUE IF MOVE IS POSSIBLE
        public bool move_is_possible(int new_position_i, int new_position_j, string piece_color)
        {
            //IMPOSSIBLE TO TAKE A KING PIECE
            if (engine.table[new_position_i, new_position_j].occupied)
                if (engine.piece_collection[engine.table[new_position_i, new_position_j].piece_i, engine.table[new_position_i, new_position_j].piece_j].this_role == "king"
                    && engine.piece_collection[engine.table[new_position_i, new_position_j].piece_i, engine.table[new_position_i, new_position_j].piece_j].this_color != piece_color)
                    return false;

            //COORDINATES OF SELECTED PIECE IN THE PIECE COLLECTION
            int aux_piece_i = engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].piece_i, 
                aux_piece_j = engine.table[engine.position_of_piece_selected_i, engine.position_of_piece_selected_j].piece_j;
            string aux_piece_role = engine.piece_collection[aux_piece_i, aux_piece_j].this_role;

            //DIFFERENT CASES DEPENDING ON ROLE OF PIECE
            switch(aux_piece_role) {
                case "pawn":
                    if (engine.black_turn)
                    {
                        //CHECK NORMAL POSITION
                        if (new_position_i <= engine.position_of_piece_selected_i || new_position_i > engine.position_of_piece_selected_i + 2 || new_position_j < engine.position_of_piece_selected_j - 1 || new_position_j > engine.position_of_piece_selected_j + 1)
                            return false;

                        //CHECK PAWN PIECE TAKING
                        if ( new_position_j != engine.position_of_piece_selected_j
                            && (new_position_i == engine.position_of_piece_selected_i + 2 || (!engine.table[new_position_i, new_position_j].occupied && !(engine.en_passant && engine.en_passant_y == new_position_j && engine.en_passant_x == engine.position_of_piece_selected_i))))
                            return false;

                        //CHECK PAWN ADVANCING
                        if (new_position_j == engine.position_of_piece_selected_j)
                            for (int k = engine.position_of_piece_selected_i + 1; k <= new_position_i; ++k)
                                if (engine.table[k, new_position_j].occupied)
                                    return false;
                        if (new_position_i - engine.position_of_piece_selected_i == 2 && new_position_i != 3)
                            return false;
                    }
                    else
                    {
                        //CHECK NORMAL POSITION
                        if (new_position_i >= engine.position_of_piece_selected_i || new_position_i < engine.position_of_piece_selected_i - 2 || new_position_j < engine.position_of_piece_selected_j - 1 || new_position_j > engine.position_of_piece_selected_j + 1)
                            return false;

                        //CHECK PAWN PIECE TAKING
                        if (new_position_j != engine.position_of_piece_selected_j
                            && (new_position_i == engine.position_of_piece_selected_i - 2 || (!engine.table[new_position_i, new_position_j].occupied && !(engine.en_passant && engine.en_passant_y == new_position_j && engine.en_passant_x == engine.position_of_piece_selected_i))))
                            return false;

                        //CHECK PAWN ADVANCING
                        if (new_position_j == engine.position_of_piece_selected_j)
                            for (int k = engine.position_of_piece_selected_i - 1; k >= new_position_i; --k)
                                if (engine.table[k, new_position_j].occupied)
                                    return false;
                        if (engine.position_of_piece_selected_i - new_position_i == 2 && new_position_i != 4)
                            return false;
                    }

                    break;
                case "knight":
                    //CHECK NORMAL POSITION
                    bool ok = false;
                    for (int k = 0; k < 8; ++k)
                        if (engine.knight_dx[k] == new_position_i - engine.position_of_piece_selected_i && engine.knight_dy[k] == new_position_j - engine.position_of_piece_selected_j)
                            ok = true;
                    if (!ok)
                        return false;

                    break;
                case "bishop":
                    //CHECK NORMAL POSITION - SAME DIAGONAL
                    if (new_position_i - engine.position_of_piece_selected_i != new_position_j - engine.position_of_piece_selected_j && new_position_i + new_position_j != engine.position_of_piece_selected_i + engine.position_of_piece_selected_j)
                        return false;

                    //DIAGONAL TYPE "\"
                    if (new_position_i - engine.position_of_piece_selected_i == new_position_j - engine.position_of_piece_selected_j)
                    {
                        if (new_position_i < engine.position_of_piece_selected_i)
                        {
                            for(int k = 1; k < engine.position_of_piece_selected_i - new_position_i; ++k)
                                if(engine.table[engine.position_of_piece_selected_i - k, engine.position_of_piece_selected_j - k].occupied)
                                    return false;
                        }
                        else if (new_position_i > engine.position_of_piece_selected_i)
                        {
                            for (int k = 1; k < new_position_i - engine.position_of_piece_selected_i; ++k)
                                if (engine.table[engine.position_of_piece_selected_i + k, engine.position_of_piece_selected_j + k].occupied)
                                    return false;
                        }
                    }

                    //DIAGONAL TYPE "/"
                    if (new_position_i + new_position_j == engine.position_of_piece_selected_i + engine.position_of_piece_selected_j)
                    {
                        if (new_position_i < engine.position_of_piece_selected_i)
                        {
                            for (int k = 1; k < engine.position_of_piece_selected_i - new_position_i; ++k)
                                if (engine.table[engine.position_of_piece_selected_i - k, engine.position_of_piece_selected_j + k].occupied)
                                    return false;
                        }
                        else if (new_position_i > engine.position_of_piece_selected_i)
                        {
                            for (int k = 1; k < new_position_i - engine.position_of_piece_selected_i; ++k)
                                if (engine.table[engine.position_of_piece_selected_i + k, engine.position_of_piece_selected_j - k].occupied)
                                    return false;
                        }
                    }


                    break;
                case "rook":
                    //CHECK NORMAL POSITION - STRAIGHT LINE
                    if (new_position_i != engine.position_of_piece_selected_i && new_position_j != engine.position_of_piece_selected_j)
                        return false;
                    
                    //HORIZONTAL MOVE
                    if (new_position_i == engine.position_of_piece_selected_i)
                    {
                        if(new_position_j < engine.position_of_piece_selected_j)
                        {
                            for(int k = engine.position_of_piece_selected_j - 1; k > new_position_j; --k)
                                if(engine.table[new_position_i, k].occupied)
                                    return false;
                        }
                        else if(new_position_j > engine.position_of_piece_selected_j)
                        {
                            for(int k = engine.position_of_piece_selected_j + 1; k < new_position_j; ++k)
                                if(engine.table[new_position_i, k].occupied)
                                    return false;
                        }
                    }

                    //VERTICAL MOVE
                    if (new_position_j == engine.position_of_piece_selected_j)
                    {
                        if (new_position_i < engine.position_of_piece_selected_i)
                        {
                            for (int k = engine.position_of_piece_selected_i - 1; k > new_position_i; --k)
                                if (engine.table[k, new_position_j].occupied)
                                    return false;
                        }
                        else if (new_position_i > engine.position_of_piece_selected_i)
                        {
                            for (int k = engine.position_of_piece_selected_i + 1; k < new_position_i; ++k)
                                if (engine.table[k, new_position_j].occupied)
                                    return false;
                        }
                    }

                    break;
                case "queen":
                    //CHECK NORMAL POSITION
                    if (new_position_i - engine.position_of_piece_selected_i != new_position_j - engine.position_of_piece_selected_j && new_position_i + new_position_j != engine.position_of_piece_selected_i + engine.position_of_piece_selected_j // DIAGONAL LINE
                        && new_position_i != engine.position_of_piece_selected_i && new_position_j != engine.position_of_piece_selected_j) //STRAIGHT LINE
                        return false;
                    else
                    {
                        //DIAGONAL TYPE "\"
                        if (new_position_i - engine.position_of_piece_selected_i == new_position_j - engine.position_of_piece_selected_j)
                        {
                            if (new_position_i < engine.position_of_piece_selected_i)
                            {
                                for (int k = 1; k < engine.position_of_piece_selected_i - new_position_i; ++k)
                                    if (engine.table[engine.position_of_piece_selected_i - k, engine.position_of_piece_selected_j - k].occupied)
                                        return false;
                            }
                            else if (new_position_i > engine.position_of_piece_selected_i)
                            {
                                for (int k = 1; k < new_position_i - engine.position_of_piece_selected_i; ++k)
                                    if (engine.table[engine.position_of_piece_selected_i + k, engine.position_of_piece_selected_j + k].occupied)
                                        return false;
                            }
                        }

                        //DIAGONAL TYPE "/"
                        if (new_position_i + new_position_j == engine.position_of_piece_selected_i + engine.position_of_piece_selected_j)
                        {
                            if (new_position_i < engine.position_of_piece_selected_i)
                            {
                                for (int k = 1; k < engine.position_of_piece_selected_i - new_position_i; ++k)
                                    if (engine.table[engine.position_of_piece_selected_i - k, engine.position_of_piece_selected_j + k].occupied)
                                        return false;
                            }
                            else if (new_position_i > engine.position_of_piece_selected_i)
                            {
                                for (int k = 1; k < new_position_i - engine.position_of_piece_selected_i; ++k)
                                    if (engine.table[engine.position_of_piece_selected_i + k, engine.position_of_piece_selected_j - k].occupied)
                                        return false;
                            }
                        }
                        
                        //HORIZONTAL MOVE
                        if (new_position_i == engine.position_of_piece_selected_i)
                        {
                            if (new_position_j < engine.position_of_piece_selected_j)
                            {
                                for (int k = engine.position_of_piece_selected_j - 1; k > new_position_j; --k)
                                    if (engine.table[new_position_i, k].occupied)
                                        return false;
                            }
                            else if (new_position_j > engine.position_of_piece_selected_j)
                            {
                                for (int k = engine.position_of_piece_selected_j + 1; k < new_position_j; ++k)
                                    if (engine.table[new_position_i, k].occupied)
                                        return false;
                            }
                        }

                        //VERTICAL MOVE
                        if (new_position_j == engine.position_of_piece_selected_j)
                        {
                            if (new_position_i < engine.position_of_piece_selected_i)
                            {
                                for (int k = engine.position_of_piece_selected_i - 1; k > new_position_i; --k)
                                    if (engine.table[k, new_position_j].occupied)
                                        return false;
                            }
                            else if (new_position_i > engine.position_of_piece_selected_i)
                            {
                                for (int k = engine.position_of_piece_selected_i + 1; k < new_position_i; ++k)
                                    if (engine.table[k, new_position_j].occupied)
                                        return false;
                            }
                        }
                    }

                    break;
                case "king":
                    //CHECK NORMAL POSITION
                    if (new_position_i < engine.position_of_piece_selected_i - 1 || new_position_i > engine.position_of_piece_selected_i + 1 || new_position_j < engine.position_of_piece_selected_j - 1 || new_position_j > engine.position_of_piece_selected_j + 1)
                        return false;

                    break;
                default:
                    MessageBox.Show("Error at move_is_possible switch");
                    break;
            }

            return true;
        }
    }

    public class piece
    {
        public int x, y;
        public bool active, has_been_moved;
        public string this_color, this_role;
    }

    public class position : Button
    {
        public bool occupied;
        public int piece_i, piece_j, x, y;
    }
}
