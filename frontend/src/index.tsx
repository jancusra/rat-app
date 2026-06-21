import ReactDOM from "react-dom/client";
import RatApp from './RatApp';
import './css/styles';

// should prevent double rendering in development mode
const root = document.getElementById('root');

if (root) {
  ReactDOM.createRoot(root).render(
    <RatApp/>
  );
}