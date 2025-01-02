export function clipboard(node: Node, text: string) {
	let error: any | null;
	const handleClick = () => {
		try {
			navigator.clipboard.writeText(text);
			node.dispatchEvent(new CustomEvent('success', {}));
		} catch (e) {
			node.dispatchEvent(new CustomEvent('failed', { detail: { error } }));
			error = e;
		}
	};

	node.addEventListener('click', handleClick);

	return {
		update(newText: string) {
			text = newText;
		},
		destroy() {
			node.removeEventListener('click', handleClick);
		}
	};
}
