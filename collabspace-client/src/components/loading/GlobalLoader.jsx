import { useLocation } from 'react-router-dom';
import useGlobalLoading from '../../hooks/useGlobalLoading';

function GlobalLoader() {
    const location = useLocation();
    const { isLoading } = useGlobalLoading();
    const shouldAnimateRouteLoader = location.key !== 'default';

    return (
        <>
            <div className={`global-progress ${isLoading ? 'visible' : ''}`}>
              
            </div>

            {shouldAnimateRouteLoader && (
                <div key={location.key} className="route-loader route-loader-animate">
                    <div className="route-loader-card">
                        <div className="route-loader-mark">
                            <span className="route-loader-core" />
                            <span className="route-loader-orbit orbit-one" />
                            <span className="route-loader-orbit orbit-two" />
                            <span className="route-loader-orbit orbit-three" />
                        </div>
                        <div className="route-loader-copy">
                            <p className="route-loader-title">CollabSpace</p>
                            <p className="route-loader-subtitle">
                                Loading your workspace flow
                            </p>
                        </div>
                    </div>
                </div>
            )}
        </>
    );
}

export default GlobalLoader;
