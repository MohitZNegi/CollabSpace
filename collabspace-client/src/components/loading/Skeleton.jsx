function Skeleton({ className = '' }) {
    return <div className={`cs-skeleton ${className}`.trim()} aria-hidden="true" />;
}

export default Skeleton;
