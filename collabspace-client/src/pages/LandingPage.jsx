import { useNavigate } from 'react-router-dom';
import '../styles/components/landing.css';

function LandingPage() {
    const navigate = useNavigate();

    return (
        <div className="landing-page">
            <nav className="landing-nav">
                <span className="landing-logo">CollabSpace</span>
                <div className="landing-nav-links">
                    <button
                        className="btn-secondary"
                        onClick={() => navigate('/login')}
                    >
                        Sign in
                    </button>
                    <button
                        className="btn-primary"
                        onClick={() => navigate('/login')}
                    >
                        Get started free
                    </button>
                </div>
            </nav>

            <section className="landing-hero">
                <div className="landing-hero-content">
                    <h1 className="landing-headline">
                        Your team's work,<br />
                        <span className="landing-headline-accent">
                            all in one place
                        </span>
                    </h1>
                    <p className="landing-subheadline">
                        CollabSpace brings together task management,
                        real-time chat, and team collaboration into
                        one unified workspace. No more switching between
                        ten different tools.
                    </p>
                    <div className="landing-cta-group">
                        <button
                            className="landing-cta-primary"
                            onClick={() => navigate('/login')}
                        >
                            Start for free
                        </button>
                        <button
                            className="landing-cta-secondary"
                            onClick={() => navigate('/about')}
                        >
                            Learn more
                        </button>
                    </div>
                </div>
            </section>

            <section className="landing-features">
                <h2 className="landing-section-title">
                    Everything your team needs
                </h2>
                <div className="landing-features-grid">
                    {[
                        
                            {
                                icon: '&#9671;',
                                title: 'Visual Task Boards',
                                desc: 'Organize your work with simple drag-and-drop boards. Move tasks, track progress, and keep everything in one place.'
                            },
                            {
                                icon: '&#128172;',
                                title: 'Built-in Team Chat',
                                desc: 'Chat with your team instantly without switching apps. Keep conversations and work connected in one space.'
                            },
                            {
                                icon: '&#128276;',
                                title: 'Stay in the Loop',
                                desc: 'Get notified when something important happens—like task updates, mentions, or new activity.'
                            },
                            {
                                icon: '&#128101;',
                                title: 'Team Collaboration',
                                desc: 'Work together with your team by assigning tasks, sharing updates, and staying aligned on goals.'
                            },
                            {
                                icon: '&#128202;',
                                title: 'Workspace Overview',
                                desc: 'Quickly see what’s happening across your workspace with a clear view of tasks and team activity.'
                            },
                            {
                                icon: '&#128274;',
                                title: 'Safe & Reliable',
                                desc: 'Your data is protected so you can focus on your work without worrying about security.'
                            },
                        
                    ].map((feature, i) => (
                        <div key={i} className="feature-card">
                            <div className="feature-icon">
                                <span dangerouslySetInnerHTML={{
                                    __html: feature.icon
                                }} />
                            </div>
                            <h3 className="feature-title">
                                {feature.title}
                            </h3>
                            <p className="feature-desc">{feature.desc}</p>
                        </div>
                    ))}
                </div>
            </section>

            <section className="landing-cta-section">
                <h2>Ready to bring your team together?</h2>
                <p>
                    Free to use. No credit card required.
                </p>
                <button
                    className="landing-cta-primary"
                    onClick={() => navigate('/login')}
                >
                    Create your workspace
                </button>
            </section>

            <footer className="landing-footer">
                <span>&#169; 2026 CollabSpace</span>
            </footer>
        </div>
    );
}

export default LandingPage;