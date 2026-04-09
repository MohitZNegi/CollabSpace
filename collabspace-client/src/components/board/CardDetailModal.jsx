import { useState } from 'react';
import axiosInstance from '../../api/axiosInstance';
import CommentSection from './CommentSection';


function CardDetailModal({ card, onClose, onUpdated }) {
    const [title, setTitle] = useState(card.title);
    const [description, setDescription] = useState(card.description || '');
    const [status, setStatus] = useState(card.status);
    const [isSaving, setIsSaving] = useState(false);

    const hasChanges = title !== card.title
        || description !== (card.description || '')
        || status !== card.status;

    const handleSave = async () => {
        if (!hasChanges || isSaving) return;
        setIsSaving(true);

        try {
            const response = await axiosInstance.put(`/cards/${card.id}`, {
                title,
                description,
                status,
                assignedToUserId: card.assignedToUserId,
            });
            // Notify the parent so it can update the card in Redux state.
            // The SignalR broadcast will also update other users' boards.
            onUpdated(response.data);
            onClose();
        } catch (error) {
            console.error('Failed to update card:', error);
        } finally {
            setIsSaving(false);
        }
    };

    const statusButtons = [
        { value: 'Todo', label: 'Todo', activeClass: 'active-todo' },
        { value: 'InProgress', label: 'In Progress', activeClass: 'active-inprogress' },
        { value: 'Done', label: 'Done', activeClass: 'active-done' },
    ];

    return (
        // Clicking the overlay closes the modal.
        // e.stopPropagation() on the modal itself prevents
        // clicks inside from bubbling up to the overlay.
        <div className="card-modal-overlay" onClick={onClose}>
            <div className="card-modal" onClick={(e) => e.stopPropagation()}>

                <div className="card-modal-header">
                    <input
                        className="card-modal-title-input"
                        value={title}
                        onChange={(e) => setTitle(e.target.value)}
                        placeholder="Card title"
                    />
                    <button className="card-modal-close" onClick={onClose}>
                        &#10005;
                    </button>
                </div>

                <div className="card-modal-body">
                    <div className="card-modal-main">
                        <div>
                            <label className="card-field-label">Description</label>
                            <textarea
                                className="card-description-input"
                                value={description}
                                onChange={(e) => setDescription(e.target.value)}
                                placeholder="Add a description..."
                                rows={4}
                            />
                        </div>
                        <CommentSection cardId={card.id} />
                    </div>

                    <div className="card-modal-sidebar">
                        <div>
                            <label className="card-field-label">Status</label>
                            <div className="card-status-group">
                                {statusButtons.map((btn) => (
                                    <button
                                        key={btn.value}
                                        className={`status-btn ${status === btn.value
                                                ? btn.activeClass : ''}`}
                                        onClick={() => setStatus(btn.value)}
                                    >
                                        {btn.label}
                                    </button>
                                ))}
                            </div>
                        </div>

                        <div>
                            <label className="card-field-label">Created by</label>
                            <span style={{
                                fontSize: 'var(--font-size-sm)',
                                color: 'var(--color-text-muted)'
                            }}>
                                {card.createdByUsername}
                            </span>
                        </div>
                    </div>
                </div>

                <div className="card-modal-actions">
                    <button className="btn-secondary" onClick={onClose}>
                        Cancel
                    </button>
                    <button
                        className="btn-primary"
                        onClick={handleSave}
                        disabled={!hasChanges || isSaving}
                    >
                        {isSaving ? 'Saving...' : 'Save changes'}
                    </button>
                </div>
            </div>
        </div>
    );
}

export default CardDetailModal;