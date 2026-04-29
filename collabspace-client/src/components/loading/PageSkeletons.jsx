import Skeleton from './Skeleton';

export function DashboardPageSkeleton() {
    return (
        <div className="dashboard-container">
            <nav className="dashboard-nav">
                <Skeleton className="cs-skeleton-text nav-brand-skeleton" />
                <div className="nav-right">
                    <Skeleton className="cs-skeleton-pill" />
                    <Skeleton className="cs-skeleton-circle" />
                    <Skeleton className="cs-skeleton-button" />
                </div>
            </nav>

            <main className="dashboard-main">
                <div className="dashboard-welcome cs-skeleton-panel">
                    <Skeleton className="cs-skeleton-title" />
                    <Skeleton className="cs-skeleton-text short" />
                </div>

                <div className="section-header">
                    <Skeleton className="cs-skeleton-text medium" />
                    <div className="cs-skeleton-actions">
                        <Skeleton className="cs-skeleton-button" />
                        <Skeleton className="cs-skeleton-button" />
                    </div>
                </div>

                <div className="workspace-grid">
                    {Array.from({ length: 6 }).map((_, index) => (
                        <div key={index} className="workspace-card cs-skeleton-card">
                            <Skeleton className="cs-skeleton-text medium" />
                            <Skeleton className="cs-skeleton-text" />
                            <Skeleton className="cs-skeleton-text short" />
                            <div className="workspace-card-footer">
                                <Skeleton className="cs-skeleton-pill" />
                                <Skeleton className="cs-skeleton-text short" />
                            </div>
                        </div>
                    ))}
                </div>
            </main>
        </div>
    );
}

export function WorkspacePageSkeleton() {
    return (
        <div className="workspace-page">
            <header className="workspace-header">
                <div className="workspace-header-left">
                    <Skeleton className="cs-skeleton-button ghost" />
                    <Skeleton className="cs-skeleton-text medium inverse" />
                </div>
                <div className="workspace-header-right">
                    <Skeleton className="cs-skeleton-circle inverse" />
                    <Skeleton className="cs-skeleton-button ghost" />
                </div>
            </header>

            <main className="workspace-main">
                <div className="invite-banner cs-skeleton-panel">
                    <div className="cs-skeleton-stack">
                        <Skeleton className="cs-skeleton-text short" />
                        <Skeleton className="cs-skeleton-title medium" />
                    </div>
                    <Skeleton className="cs-skeleton-button" />
                </div>

                <div className="stats-bar">
                    {Array.from({ length: 3 }).map((_, index) => (
                        <div key={index} className="stat-card cs-skeleton-card">
                            <Skeleton className="cs-skeleton-title centered" />
                            <Skeleton className="cs-skeleton-text short centered" />
                        </div>
                    ))}
                </div>

                <div className="activity-feed">
                    <div className="activity-feed-header">
                        <Skeleton className="cs-skeleton-text medium" />
                    </div>
                    <div className="activity-list">
                        {Array.from({ length: 4 }).map((_, index) => (
                            <div key={index} className="activity-item">
                                <Skeleton className="cs-skeleton-circle" />
                                <div className="activity-content cs-skeleton-stack">
                                    <Skeleton className="cs-skeleton-text" />
                                    <Skeleton className="cs-skeleton-text short" />
                                </div>
                            </div>
                        ))}
                    </div>
                </div>

                <div className="section-header">
                    <Skeleton className="cs-skeleton-text medium" />
                </div>
                <div className="boards-grid">
                    {Array.from({ length: 4 }).map((_, index) => (
                        <div key={index} className="board-card cs-skeleton-card">
                            <Skeleton className="cs-skeleton-text medium" />
                            <Skeleton className="cs-skeleton-text short" />
                        </div>
                    ))}
                </div>

                <div className="members-section">
                    <Skeleton className="cs-skeleton-text medium" />
                    <div className="members-list">
                        {Array.from({ length: 4 }).map((_, index) => (
                            <div key={index} className="member-row">
                                <Skeleton className="cs-skeleton-circle" />
                                <div className="member-info cs-skeleton-stack">
                                    <Skeleton className="cs-skeleton-text short" />
                                    <Skeleton className="cs-skeleton-text short" />
                                </div>
                                <Skeleton className="cs-skeleton-pill" />
                            </div>
                        ))}
                    </div>
                </div>
            </main>
        </div>
    );
}

export function BoardPageSkeleton() {
    return (
        <div className="board-page">
            <header className="board-header">
                <div className="board-header-left">
                    <Skeleton className="cs-skeleton-button ghost" />
                    <Skeleton className="cs-skeleton-text medium inverse" />
                </div>
                <div className="board-header-right">
                    <Skeleton className="cs-skeleton-circle inverse" />
                    <Skeleton className="cs-skeleton-button ghost" />
                </div>
            </header>

            <div className="board-body">
                <div className="board-columns">
                    {Array.from({ length: 3 }).map((_, index) => (
                        <div key={index} className="board-column board-column-skeleton">
                            <div className="column-header">
                                <Skeleton className="cs-skeleton-text short" />
                                <Skeleton className="cs-skeleton-pill small" />
                            </div>
                            <div className="column-cards">
                                {Array.from({ length: 4 }).map((__, cardIndex) => (
                                    <div key={cardIndex} className="card-item cs-skeleton-card">
                                        <Skeleton className="cs-skeleton-text" />
                                        <Skeleton className="cs-skeleton-text short" />
                                        <div className="card-footer">
                                            <Skeleton className="cs-skeleton-pill small" />
                                            <Skeleton className="cs-skeleton-pill small" />
                                        </div>
                                    </div>
                                ))}
                            </div>
                            <div className="add-card-btn skeleton-add-card">
                                <Skeleton className="cs-skeleton-text short" />
                            </div>
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}

export function CommentSectionSkeleton() {
    return (
        <div className="comments-list">
            {Array.from({ length: 3 }).map((_, index) => (
                <div key={index} className="comment-item">
                    <Skeleton className="cs-skeleton-circle small" />
                    <div className="comment-body cs-skeleton-stack">
                        <div className="comment-header">
                            <Skeleton className="cs-skeleton-text short" />
                            <Skeleton className="cs-skeleton-text short" />
                        </div>
                        <Skeleton className="cs-skeleton-text" />
                        <Skeleton className="cs-skeleton-text" />
                        <Skeleton className="cs-skeleton-text short" />
                    </div>
                </div>
            ))}
        </div>
    );
}
