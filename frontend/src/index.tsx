import ReactDOM from "react-dom/client";
import RatApp from './RatApp';
import './css/styles.css';

const root = document.getElementById('root');

if (root) {
    ReactDOM.createRoot(root).render(
        <RatApp />
    );
}