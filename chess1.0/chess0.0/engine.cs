  using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess0._0
{
    public static class engine
    {
        public static bool black_turn, piece_selected, flip;
        public static int position_of_piece_selected_i, position_of_piece_selected_j;
        public static position[,] table;
        public static piece[,] piece_collection;
        public static string[] role = { "pawn", "rook", "knight", "bishop", "queen", "king" };

        public static int[] knight_dx = {2, 2, 1, -1, -2, -2, -1, 1}, 
                            knight_dy = {-1, 1, 2, 2, 1, -1, -2, -2},
                            king_dx = {-1, -1, -1, 0, 1, 1, 1, 0}, 
                            king_dy = {-1, 0, 1, 1, 1, 0, -1, -1};

        public static bool en_passant;
        public static int en_passant_x, en_passant_y;

        public static int promotion_x, promotion_y;
    }
}
