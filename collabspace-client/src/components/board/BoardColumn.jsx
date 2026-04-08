import { useState } from 'react';
import { Droppable } from '@hello-pangea/dnd';
import CardItem from './CardItem';

function BoardColumn({ column, cards, onCardClick, onAddCard }) {
    const [isAddingCard, setIsAddingCard] = useState(false);
    const [newCardTitle, setNewCardTitle] = useState('');

    const handleAddCard = () => {
        const title = newCardTitle.trim();
        if (!title) {
            setIsAddingCard(false);
            return;
        }
        onAddCard(column.id, title);
        setNewCardTitle('');
        setIsAddingCard(false);
    };

    const handleKeyDown = (e) => {
        if (e.key === 'Enter') { e.preventDefault(); handleAddCard(); }
        if (e.key === 'Escape') { setIsAddingCard(false); setNewCardTitle(''); }
    };

    return (
        <div className="board-column">
            <div className="column-header">
                <span className="column-title">{column.label}</span>
                <span className="column-count">{cards.length}</span>
            </div>

            {/* Droppable defines a drop target area.
                droppableId must match the column status value
                so the drop handler knows which column received the card */}
            <Droppable droppableId={column.id}>
                {(provided, snapshot) => (
                    <div
                        ref={provided.innerRef}
                        {...provided.droppableProps}
                        className={`column-cards ${snapshot.isDraggingOver ? 'dragging-over' : ''}`}
                    >
                        {cards.map((card, index) => (
                            <CardItem
                                key={card.id}
                                card={card}
                                index={index}
                                onClick={onCardClick}
                            />
                        ))}
                        {/* Placeholder maintains column height during drag */}
                        {provided.placeholder}
                    </div>
                )}
            </Droppable>

            {isAddingCard ? (
                <div className="new-card-form">
                    <textarea
                        className="new-card-input"
                        placeholder="Enter card title..."
                        value={newCardTitle}
                        onChange={(e) => setNewCardTitle(e.target.value)}
                        onKeyDown={handleKeyDown}
                        autoFocus
                        rows={2}
                    />
                    <div className="new-card-actions">
                        <button className="btn-primary" onClick={handleAddCard}>
                            Add card
                        </button>
                        <button className="btn-secondary"
                            onClick={() => {
                                setIsAddingCard(false);
                                setNewCardTitle('');
                            }}>
                            Cancel
                        </button>
                    </div>
                </div>
            ) : (
                <button
                    className="add-card-btn"
                    onClick={() => setIsAddingCard(true)}
                >
                    + Add a card
                </button>
            )}
        </div>
    );
}

export default BoardColumn;