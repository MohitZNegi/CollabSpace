import { createSlice } from '@reduxjs/toolkit';

const boardSlice = createSlice({
    name: 'board',
    initialState: {
        boards: [],
        currentBoard: null,
        cards: [],
        activeUsers: [],
        isLoading: false,
        error: null,
    },
    reducers: {
        setBoards: (state, action) => {
            state.boards = action.payload;
        },
        setCurrentBoard: (state, action) => {
            state.currentBoard = action.payload;
        },
        setCards: (state, action) => {
            state.cards = action.payload;
        },

        // Called when SignalR broadcasts CardCreated.
        // Adds the new card to local state without an API call.
        cardCreated: (state, action) => {
            state.cards.push(action.payload);
        },

        // Called when SignalR broadcasts CardUpdated.
        // Finds the card by ID and replaces it with updated data.
        cardUpdated: (state, action) => {
            const index = state.cards
                .findIndex(c => c.id === action.payload.id);
            if (index !== -1) {
                state.cards[index] = action.payload;
            }
        },

        // Called when SignalR broadcasts CardMoved.
        // Updates only status and position — lightweight.
        cardMoved: (state, action) => {
            const { cardId, status, position } = action.payload;
            const card = state.cards.find(c => c.id === cardId);
            if (card) {
                card.status = status;
                card.position = position;
            }
        },

        // Called when SignalR broadcasts CardDeleted.
        cardDeleted: (state, action) => {
            state.cards = state.cards
                .filter(c => c.id !== action.payload.cardId);
        },

        // Track who is currently viewing this board.
        userJoinedBoard: (state, action) => {
            const exists = state.activeUsers
                .some(u => u.userId === action.payload.userId);
            if (!exists) state.activeUsers.push(action.payload);
        },
        userLeftBoard: (state, action) => {
            state.activeUsers = state.activeUsers
                .filter(u => u.userId !== action.payload.userId);
        },

        setLoading: (state, action) => {
            state.isLoading = action.payload;
        },
        setError: (state, action) => {
            state.error = action.payload;
        },
        clearBoard: (state) => {
            state.currentBoard = null;
            state.cards = [];
            state.activeUsers = [];
        }
    }
});

export const {
    setBoards, setCurrentBoard, setCards,
    cardCreated, cardUpdated, cardMoved, cardDeleted,
    userJoinedBoard, userLeftBoard,
    setLoading, setError, clearBoard
} = boardSlice.actions;

export default boardSlice.reducer;