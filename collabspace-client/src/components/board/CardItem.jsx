import { Draggable } from '@hello-pangea/dnd';

function CardItem({ card, index, onClick }) {
    return (
        // Draggable wraps each card. draggableId must be a unique string.
        // index is required by the library to track position during drag.
        <Draggable draggableId={card.id} index={index}>
            {(provided, snapshot) => (
                <div
                    ref={provided.innerRef}
                    {...provided.draggableProps}
                    {...provided.dragHandleProps}
                    className={`card-item ${snapshot.isDragging ? 'dragging' : ''}`}
                    onClick={() => onClick(card)}
                >
                    <div className="card-title">{card.title}</div>
                    <div className="card-footer">
                        {card.assignedToUsername ? (
                            <div className="card-assignee">
                                <div className="card-assignee-avatar">
                                    {card.assignedToUsername[0].toUpperCase()}
                                </div>
                                <span>{card.assignedToUsername}</span>
                            </div>
                        ) : (
                            <span />
                        )}
                        {card.commentCount > 0 && (
                            <span className="card-comment-count">
                                &#128172; {card.commentCount}
                            </span>
                        )}
                    </div>
                </div>
            )}
        </Draggable>
    );
}

export default CardItem;