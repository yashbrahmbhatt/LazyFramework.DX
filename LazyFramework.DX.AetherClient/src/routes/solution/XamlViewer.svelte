<script lang="ts" module>
	export enum NodeSearchScope {
		Descendants,
		Parents,
		Ancestors,
		Children
	}

	// Define types for nodes and edges
	type NodeType = string; // Can be more specific if needed (e.g., 'Start', 'Action', 'Condition')
	type EdgeType = string; // A string like 'A --> B'

	// Define the structure for the Mermaid chart
	interface MermaidChart {
		nodes: NodeType[];
		edges: EdgeType[];
	}

	// Type for traversing the XML using the DOMParser
	interface XMLNode extends Element {
		childNodes: NodeListOf<ChildNode>;
	}

	interface Argument {
		name: string;
		type: string;
		value: string;
		direction: ArgumentDirection;
	}

	enum ArgumentDirection {
		In,
		Out
	}

	interface ActivityNode {
		id: string;
		type: string;
		displayName: string;
		arguments: Argument[];
		children: Record<string, ActivityNode>;
	}

	function getDescendantsWithCondition(node: Node, condition: (node: Node) => boolean): Node[] {
		let nodes: Node[] = [];
		if (condition(node)) {
			nodes.push(node);
		}
		Array.from(node.childNodes).forEach((childNode: Node) => {
			nodes = [...nodes, ...getDescendantsWithCondition(childNode, condition)];
		});
		return nodes;
	}

	function getAncestorsWithCondition(node: Node, condition: (node: Node) => boolean): Node[] {
		let nodes: Node[] = [];
		if (condition(node)) {
			nodes.push(node);
		}
		if (node.parentNode) {
			nodes = [...nodes, ...getAncestorsWithCondition(node.parentNode, condition)];
		}
		return nodes;
	}

	function isActivity(node: Node): boolean {
		if (node.nodeType === Node.ELEMENT_NODE) {
			const element = node as Element;
			const displayName = element.attributes.getNamedItem('DisplayName');
			const id = element.attributes.getNamedItem('sap2010:WorkflowViewState.IdRef');
			return displayName !== null && id !== null;
		}
		return false;
	}

	function isRoot(node: Node): boolean {
		if (node.nodeType === Node.ELEMENT_NODE) {
			const element = node as Element;
			const className = element.attributes.getNamedItem('x:Class');
			return className !== null;
		}
		return false;
	}

	function getPath(child: Node, parent: Node, path: string): string {
		const ancestors = getAncestorsWithCondition(child, (node) => true);
		const parentAncestors = getAncestorsWithCondition(parent, (node) => true);
		const notCommonAncestors = ancestors.filter((ancestor) => !parentAncestors.includes(ancestor));
		return notCommonAncestors.reduce((acc, curr) => {
			const element = curr as Element;
			const nodeName = element.nodeName;
			let nodeIndex = 0;
			if (element.parentElement) {
				const siblings = Array.from(element.parentElement.childNodes).filter(
					(node) => node.nodeName === nodeName
				);
				nodeIndex = siblings.indexOf(curr as ChildNode);
			}
			return `${nodeName}[${nodeIndex}].${acc}`;
		}, '');
	}

	function getDepth(child: Node, parent: Node): number {
		const ancestors = getAncestorsWithCondition(child, (node) => true);
		const parentAncestors = getAncestorsWithCondition(parent, (node) => true);
		const notCommonAncestors = parentAncestors.filter((ancestor) => !ancestors.includes(ancestor));
		return notCommonAncestors.length;
	}

	function traverseXml(xmlDoc: Node) {
		if (xmlDoc.nodeType === Node.ELEMENT_NODE) {
			const element = xmlDoc as Element;
			const className = element.attributes.getNamedItem('x:Class')?.value;
			const displayName = element.attributes.getNamedItem('DisplayName')?.value;
			const id = element.attributes.getNamedItem('sap2010:WorkflowViewState.IdRef')?.value;
			if ((displayName && id) || className) {
				// console.log('Processing activity node: ', displayName, id, className);
				const activityNode: ActivityNode = {
					id: id ?? 'Activity',
					type: element.nodeName,
					displayName: className ?? displayName ?? '',
					arguments: [],
					children: {}
				};
				// Add arguments
				var argumentDescendants = getDescendantsWithCondition(
					xmlDoc,
					(node) => node.nodeName.includes('InArgument') || node.nodeName.includes('OutArgument')
				).filter((node) => {
					var ancestors = getAncestorsWithCondition(node, (node) => isActivity(node));
					if (ancestors.length > 0) {
						if (ancestors[0] === xmlDoc) {
							return true;
						}
					}
					return false;
				});
				argumentDescendants.forEach((argumentNode) => {
					console.log('Argument', argumentNode);
					const argumentElement = argumentNode as Element;
					const isDelegate = argumentElement.nodeName.includes('Delegate');
					const hasValue = argumentElement.children.length > 0;
					const argument: Argument = {
						name: getPath(argumentNode, element, ''),
						type: argumentElement.attributes.getNamedItem('x:TypeArguments')?.value ?? '',
						value: isDelegate
							? (argumentElement.attributes.getNamedItem('Name')?.value ?? '')
							: hasValue
								? argumentElement.children[0].nodeValue ?? 'null'
								: 'null',
						direction: argumentElement.nodeName.includes('InArgument')
							? ArgumentDirection.In
							: ArgumentDirection.Out
					};
					activityNode.arguments.push(argument);
				});

				// Add children
				const descendantActivities = getDescendantsWithCondition(xmlDoc, (node) =>
					isActivity(node)
				);
				// console.log('Descendants', descendantActivities);
				const childrenActivities = descendantActivities.filter((child) => {
					const ancestors = getAncestorsWithCondition(
						child,
						(node) => isActivity(node) || isRoot(node)
					);
					// console.log('Descendant ancestors', ancestors);
					if (ancestors.length > 1) {
						if (ancestors[1] === xmlDoc) {
							return true;
						}
					}
				});
				// console.log('Children', childrenActivities);
				const childrenPaths = childrenActivities.map((child) => getPath(child, element, ''));
				// console.log('Children paths', childrenPaths);
				activityNode.children = childrenPaths.reduce(
					(acc, curr) => {
						const childElement = childrenActivities.find(
							(child) => getPath(child, element, '') === curr
						) as Element;
						const childActivity = traverseXml(childElement);
						acc[curr] = childActivity as ActivityNode;
						return acc;
					},
					{} as Record<string, ActivityNode>
				);
				return activityNode;
			}
		}
		// console.log('Skipping node: ', xmlDoc.nodeName, xmlDoc.nodeType);
		return;
	}

	function convertActivityNodeToMermaid(node: ActivityNode | undefined): string {
		if (!node) return '';
		const lines: string[] = [];

		// Helper function to generate a Mermaid-compatible state identifier
		const nodeId = (node: ActivityNode) => `${node.id.replace(/[^a-zA-Z0-9]/g, '_')}`; // Ensure IDs are valid for Mermaid
		const nodeTitle = (node: ActivityNode) =>
			`${node.type.replace(/[^a-zA-Z0-9]/g, '.')} - ${node.displayName.replace('(', '').replace(')', '')}`;
		// Recursive function to process nodes and build the state diagram
		function processNode(currentNode: ActivityNode, parentId: string | null) {
			const currentNodeId = nodeId(currentNode);
			const currentNodeTitle = nodeTitle(currentNode);
			// Get the list of children
			const children = Object.values(currentNode.children);

			if (children.length > 0) {
				// Start a nested state
				lines.push(`${currentNodeId}: ${currentNodeTitle}`);
				lines.push(`state ${currentNodeId} {`);

				// Process each child node recursively
				for (let i = 0; i < children.length; i++) {
					const child = children[i];
					processNode(child, currentNodeId);

					// Connect siblings via edges
					if (i > 0) {
						const prevChildId = nodeId(children[i - 1]);
						lines.push(`${prevChildId} --> ${nodeId(child)}`);
					}
				}

				lines.push('}'); // End the nested state
			} else {
				// Add a simple state for nodes without children
				lines.push(`${currentNodeId}: ${currentNodeTitle}`);
			}

			// Connect the current node to its parent if applicable
			if (parentId) {
				// lines.push(`${parentId} --> ${currentNodeId}`);
			}
		}

		// Start processing from the root node
		lines.push('stateDiagram-v2');
		processNode(node, null);

		return lines.join('\n');
	}
</script>

<script lang="ts">
	import { ScrollArea } from '$lib/components/ui/scroll-area';

	import mermaid, { type RenderResult } from 'mermaid';
	import { onMount } from 'svelte';
	import { writable, type Writable } from 'svelte/store';

	let { xaml }: { xaml: string } = $props();
	let parser = new DOMParser();

	let doc = $derived(parser.parseFromString(xaml, 'text/xml'));
	// let mermaid = $state(generateMermaid(xaml));
	// $inspect(mermaid);

	let el: HTMLPreElement | null = null;
	let root = $derived(traverseXml(doc.documentElement));
	let mermaidStr = $derived(convertActivityNodeToMermaid(root));
	let diagram: Writable<RenderResult | null> = $state(writable<RenderResult | null>(null));
	$effect(() => renderDiagram(mermaidStr, el));

	diagram.subscribe((value) => {
		if (value && el) {
			el.innerHTML = value.svg;
			if (value.bindFunctions) {
				value.bindFunctions(el);
			}
		}
	});

	function renderDiagram(raw: string, element: HTMLElement | null): void {
		if (!element) return;
		mermaid.render('mermaid', raw, element).then((result: RenderResult) => {
			diagram.set(result);
		});
	}
	onMount(() => {
		mermaid.initialize({
			startOnLoad: true,
			altFontFamily: 'Roboto',
			darkMode: true,
			elk: {
				mergeEdges: false,
				cycleBreakingStrategy: 'DEPTH_FIRST',
				nodePlacementStrategy: 'LINEAR_SEGMENTS'
			}
		});
	});
	$inspect($diagram);
</script>

<ScrollArea orientation="vertical" class="h-full w-full">
	<pre bind:this={el} class="h-full w-full">
    {mermaidStr}
</pre>
</ScrollArea>
