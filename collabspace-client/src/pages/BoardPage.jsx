import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import { DragDropContext } from '@hello-pangea/dnd';
import toast from 'react-hot-toast';
import axiosInstance from '../api/axiosInstance';
import {
    setCards, cardUpdated, setCurrentBoard
} from '../features/board/boardSlice';
import { useBoardSignalR } from '../hooks/useBoardSignalR';
import { useChatSignalR } from '../hooks/useChatSignalR';
import BoardColumn from '../components/board/BoardColumn';
import CardDetailModal from '../components/board/CardDetailModal';
import ChatSidebar from '../components/chat/ChatSidebar';
import { BoardPageSkeleton } from '../components/loading/PageSkeletons';
import '../styles/components/board.css';

// The three columns are defined as constants.
// The id must match the Status values in your database exactly.
const COLUMNS = [
    { id: 'Todo', label: 'To Do' },
    { id: 'InProgress', label: 'In Progress' },
    { id: 'Done', label: 'Done' },
];

function BoardPage() {
    const { boardId, workspaceId } = useParams();
    const navigate = useNavigate();
    const location = useLocation();
    const dispatch = useDispatch();
    // Alias `user` to `_user` to satisfy the project's ESLint rule for allowed unused vars.
    const { user: _user } = useSelector((s) => s.auth);
    const { cards, activeUsers } = useSelector((s) => s.board);

    const [isChatOpen, setIsChatOpen] = useState(false);
    const [selectedCard, setSelectedCard] = useState(null);
    const [isLoadingCards, setIsLoadingCards] = useState(true);

    const searchParams = new URLSearchParams(location.search);
    const requestedCardId = searchParams.get('card');
    const requestedCommentId = searchParams.get('comment');
    const requestedChatMessageId = searchParams.get('chatMessage');

    // Activate SignalR listeners for board and chat events
    useBoardSignalR(boardId);
    useChatSignalR(workspaceId);

    // Load board and cards on mount
    useEffect(() => {
        const loadBoard = async () => {
            try {
                setIsLoadingCards(true);

                const [boardRes, cardsRes] = await Promise.all([
                    axiosInstance.get(`/workspaces/${workspaceId}/boards`),
                    axiosInstance.get(`/boards/${boardId}/cards`),
                ]);

                const board = boardRes.data.find(b => b.id === boardId);
                dispatch(setCurrentBoard(board || null));
                dispatch(setCards(cardsRes.data));
            } catch (error) {
                console.error(error);
                toast.error('Failed to load board.');
                navigate(`/workspaces/${workspaceId}`);
            } finally {
                setIsLoadingCards(false);
            }
        };

        loadBoard();
    }, [boardId, workspaceId, dispatch, navigate]);

    useEffect(() => {
        if (requestedChatMessageId && !isChatOpen) {
            setIsChatOpen(true);
        }
    }, [requestedChatMessageId, isChatOpen]);

    useEffect(() => {
        if (isLoadingCards || !requestedCardId || selectedCard) {
            return;
        }

        const matchedCard = cards.find((card) => card.id === requestedCardId);
        if (matchedCard) {
            setSelectedCard(matchedCard);
        }
    }, [cards, isLoadingCards, requestedCardId, selectedCard]);

    // Filter cards for a specific column, sorted by position
    const getColumnCards = useCallback((status) => {
        return cards
            .filter(c => c.status === status)
            .sort((a, b) => a.position - b.position);
    }, [cards]);

    // Handle adding a new card to a column
    const handleAddCard = async (status, title) => {
        try {
            await axiosInstance.post(`/boards/${boardId}/cards`, {
                title,
                status,
            });
            // The SignalR broadcast from the server will fire
            // cardCreated which adds it to Redux state automatically.
            // No need to manually update state here.
        } catch (error) {
            console.error(error);
            toast.error('Failed to create card.');
        }
    };

    // Handle drag end — the core of drag and drop interaction.
    // onDragEnd fires when the user releases a card.
    // result contains the dragged item's source and destination.
    const handleDragEnd = async (result) => {
        const { draggableId, source, destination } = result;

        // destination is null if the card was dropped outside a column
        if (!destination) return;

        // No movement if dropped in the same position
        if (
            source.droppableId === destination.droppableId &&
            source.index === destination.index
        ) return;

        // Optimistic update: update Redux state immediately before
        // the API call completes. The board feels instant to the user.
        // If the API call fails, we revert the change.
        const updatedCards = cards.map(card => {
            if (card.id === draggableId) {
                return {
                    ...card,
                    status: destination.droppableId,
                    position: destination.index,
                };
            }
            return card;
        });
        dispatch(setCards(updatedCards));

        try {
            await axiosInstance.patch(`/cards/${draggableId}/move`, {
                status: destination.droppableId,
                position: destination.index,
            });
            // The SignalR broadcast updates all OTHER connected users.
            // The current user already sees the change from the
            // optimistic update above.
        } catch (error) {
            console.error(error);
            // Revert to original state if the API call fails
            dispatch(setCards(cards));
            toast.error('Failed to move card. Please try again.');
        }
    };

    const { currentBoard } = useSelector((s) => s.board);

    if (isLoadingCards) {
        return <BoardPageSkeleton />;
    }

    const clearBoardQueryParams = (...keys) => {
        const nextParams = new URLSearchParams(location.search);
        keys.forEach((key) => nextParams.delete(key));

        const nextSearch = nextParams.toString();
        navigate(
            `${location.pathname}${nextSearch ? `?${nextSearch}` : ''}`,
            { replace: true }
        );
    };

    return (
        <div className="board-page">
            {/* Header */}
            <header className="board-header">
                <div className="board-header-left">
                    <button
                        className="board-back-btn"
                        onClick={() => navigate(`/workspaces/${workspaceId}`)}
                    >
                        &#8592; Back
                    </button>
                    <span className="board-title">
                        {currentBoard?.name || 'Board'}
                    </span>
                </div>

                <div className="board-header-right">
                    {/* Active user avatars */}
                    <div className="active-users">
                        {activeUsers.slice(0, 5).map((u) => (
                            <div
                                key={u.userId}
                                className="user-avatar"
                                title={u.userId}
                            >
                                {u.userId?.toString()[0]?.toUpperCase() || '?'}
                            </div>
                        ))}
                    </div>

                    <button
                        className={`chat-toggle-btn ${isChatOpen ? 'active' : ''}`}
                        onClick={() => setIsChatOpen(prev => !prev)}
                    >
                        &#128172; Chat
                    </button>
                </div>
            </header>

            {/* Body: columns + optional chat sidebar */}
            <div className="board-body">
                {/* DragDropContext wraps the entire drag-and-drop area.
                    onDragEnd is the single handler for all drop events. */}
                <DragDropContext onDragEnd={handleDragEnd}>
                    <div className="board-columns">
                        {COLUMNS.map((column) => (
                            <BoardColumn
                                key={column.id}
                                column={column}
                                cards={getColumnCards(column.id)}
                                onCardClick={setSelectedCard}
                                onAddCard={handleAddCard}
                            />
                        ))}
                    </div>
                </DragDropContext>

                {isChatOpen && (
                    <ChatSidebar
                        workspaceId={workspaceId}
                        highlightedMessageId={requestedChatMessageId}
                    />
                )}
            </div>

            {/* Card detail modal */}
            {selectedCard && (
                <CardDetailModal
                    card={selectedCard}
                    highlightedCommentId={requestedCommentId}
                    onClose={() => {
                        setSelectedCard(null);
                        clearBoardQueryParams('card', 'comment');
                    }}
                    onUpdated={(updatedCard) => {
                        dispatch(cardUpdated(updatedCard));
                        setSelectedCard(null);
                        clearBoardQueryParams('card', 'comment');
                    }}
                />
            )}
        </div>
    );
}

export default BoardPage;
