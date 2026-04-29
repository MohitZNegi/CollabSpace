import { useNavigate } from 'react-router-dom';
import '../styles/components/landing.css';

function AboutPage() {
    const navigate = useNavigate();

    return (
        <div className="landing-page">
            <nav className="landing-nav">
                <button
                    className="landing-logo"
                    onClick={() => navigate('/')}
                    style={{
                        background: 'none', border: 'none',
                        cursor: 'pointer'
                    }}
                >
                    CollabSpace
                </button>
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
                        Get started
                    </button>
                </div>
            </nav>

            <section className="about-hero">
                <h1>About CollabSpace</h1>
                <p className="landing-subheadline">
                    Built to solve the problem every team faces:
                    too many tools, not enough collaboration.
                </p>
            </section>

            <section className="about-content">
                <div className="about-block">
                    <h2>The Problem</h2>
                    <p>
                        Modern teams juggle Slack for chat, Trello for tasks,
                        Notion for docs, and email for everything else.
                        Context switching between these tools kills
                        productivity and creates communication gaps.
                    </p>
                </div>

                <div className="about-block">
                    <h2>The Solution</h2>
                    <p>
                        CollabSpace consolidates task management, real-time
                        messaging, and team collaboration into a single
                        unified platform. Everything your team needs,
                        in one place, updating in real time.
                    </p>
                </div>

                <div className="about-block">
                    <h2>Future Vision</h2>
                    <p>
                        We’re continuously improving CollabSpace by adding
                        smarter features, better integrations,
                        and enhanced real-time capabilities to
                        support modern teams as they grow.
                    </p>
                </div>

            </section>

            <footer className="landing-footer">
                <span>&#169; 2026 CollabSpace</span>
            </footer>
        </div>
    );
}

export default AboutPage;