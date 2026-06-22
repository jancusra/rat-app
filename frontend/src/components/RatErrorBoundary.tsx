import { Component, ErrorInfo, ReactNode } from 'react';
import RatLocales from '../contexts/RatLocales';
import { LocaleContext } from '../types';

type ErrorBoundaryProps = {
    children: ReactNode;
    resetKey?: string; // changing this (e.g. route) clears a previous error
}

type ErrorBoundaryState = {
    hasError: boolean;
}

class RatErrorBoundary extends Component<ErrorBoundaryProps, ErrorBoundaryState> {
    static contextType = RatLocales;

    state: ErrorBoundaryState = { hasError: false };

    static getDerivedStateFromError(): ErrorBoundaryState {
        return { hasError: true };
    }

    componentDidCatch(error: Error, info: ErrorInfo) {
        console.error("Unhandled render error", error, info);
    }

    componentDidUpdate(prevProps: ErrorBoundaryProps) {
        if (this.state.hasError && prevProps.resetKey !== this.props.resetKey) {
            this.setState({ hasError: false });
        }
    }

    render() {
        if (this.state.hasError) {
            const locales = this.context as LocaleContext;
            return <div className="rat-error-boundary">{locales.SomethingWentWrong}</div>;
        }

        return this.props.children;
    }
}

export default RatErrorBoundary;
