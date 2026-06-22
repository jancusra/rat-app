/// <reference types="webpack-env" />

declare const __RAT_API_URL__: string;

declare module '*.png' {
	const content: string;
	export default content;
}

declare module '*.svg' {
	const content: string;
	export default content;
}

declare module '*.css';