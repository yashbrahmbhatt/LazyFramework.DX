<script lang="ts" module>
</script>

<script lang="ts">
	import type { PageData } from '../solution/$types';
	import * as Resizable from '$lib/components/ui/resizable';
	import { writable } from 'svelte/store';
	import FileTree from '$lib/composites/FileTree.svelte';
	import ScrollArea from '$lib/components/ui/scroll-area/scroll-area.svelte';
	import type { Snippet } from 'svelte';
	import { Solution, type SolutionTreeNode } from './Solution';
	import {
		BookOpenText,
		Bot,
		Braces,
		Code,
		CodeIcon,
		FileIcon,
		FileJsonIcon,
		FlaskConicalIcon,
		FolderIcon,
		FolderOpenIcon,
		Settings,
		Terminal,
		TestTubeDiagonal,
		WorkflowIcon
	} from 'lucide-svelte';
	let { data }: { data: PageData } = $props();

	function parseWorkflows() {
		let files = Object.keys(data.Files);
		let workflows = files.filter((file) => file.endsWith('.xaml'));
		var parser = new DOMParser();
		var xamls = workflows.map((xaml) =>
			parser.parseFromString(data.Files[xaml], 'application/xml')
		);
		xamls.forEach((xaml, i) => {
			var activities = getFilteredDescendantElements(
				xaml,
				(element) => element.attributes.getNamedItem('DisplayName') !== null
			);
			var values = getFilteredDescendantElements(
				xaml,
				(element) => element.nodeName === 'CSharpValue' || element.nodeName === 'VisualBasicValue'
			);
			var references = getFilteredDescendantElements(
				xaml,
				(element) =>
					element.nodeName === 'CSharpReference' || element.nodeName === 'VisualBasicReference'
			);
			var properties = getFilteredDescendantElements(
				xaml,
				(element) => element.nodeName === 'x:Property'
			);
		});
		return xamls;
	}
	parseWorkflows();
	/**
	 * Recursively retrieves all element nodes that are descendants of the given node,
	 * filtered by the provided callback function.
	 *
	 * @param node - The starting node to search for descendants.
	 * @param filterCallback - A callback function to determine which elements to include.
	 *                          It receives an element node and returns `true` to include the node or `false` to exclude it.
	 * @returns An array of filtered element nodes.
	 */
	function getFilteredDescendantElements(
		node: Node,
		filterCallback: (element: Element) => boolean
	): { element: Element }[] {
		const result: { element: Element }[] = [];

		function traverse(currentNode: Node) {
			if (currentNode.nodeType === Node.ELEMENT_NODE) {
				const element = currentNode as Element;
				if (filterCallback(element)) {
					result.push({ element });
				}
			}
			currentNode.childNodes.forEach((child) => traverse(child));
		}

		traverse(node);

		return result;
	}
	let solution = new Solution(data.Files);
	let tree = $state([solution.convertSolutionToTree()]);
	let selected = $state(writable<SolutionTreeNode>());
	let icons = {
		folderOpen: FolderOpen,
		folderClosed: Folder,
		workflow: Workflow,
		project: Project,
		solution: FlaskConical,
		excel: ExcelFile,
		file: OtherFile,
		markdown: MarkdownSnippet,
		entry: EntryPoint,
		json: FileJson,
		test: TestFile,
		code: CodeSnippet
	} as Record<string, Snippet>;

	$inspect('page', $selected);
</script>

{#snippet CodeSnippet()}
	<CodeIcon class="min-h-4 min-w-4" />
{/snippet}
{#snippet Project()}
	<Braces class="min-h-4 min-w-4" />
{/snippet}
{#snippet EntryPoint()}
	<Bot class="min-h-4 min-w-4" />
{/snippet}
{#snippet FileJson()}
	<FileJsonIcon class="min-h-4 min-w-4" />
{/snippet}
{#snippet Folder()}
	<FolderIcon class="min-h-4 min-w-4" />
{/snippet}
{#snippet Workflow()}
	<WorkflowIcon class="min-h-4 min-w-4" />
{/snippet}
{#snippet FlaskConical()}
	<FlaskConicalIcon class="min-h-4 min-w-4" />
{/snippet}
{#snippet FolderOpen()}
	<FolderOpenIcon class="min-h-4 min-w-4" />
{/snippet}
{#snippet ExcelFile()}
	<Settings class="min-h-4 min-w-4" />
{/snippet}
{#snippet OtherFile()}
	<FileIcon class="min-h-4 min-w-4" />
{/snippet}
{#snippet MarkdownSnippet()}
	<BookOpenText class="min-h-4 min-w-4" />
{/snippet}
{#snippet TestFile()}
	<TestTubeDiagonal class="min-h-4 min-w-4" />
{/snippet}

<div class="flex h-[95vh] w-full flex-col place-content-center space-y-4">
	<Resizable.PaneGroup direction="horizontal" class="w-full rounded-lg border">
		<Resizable.Pane defaultSize={20}>
			<div class="flex w-full flex-col justify-center p-2">
				<ScrollArea class="h-[84vh]" orientation="vertical" scrollbarYClasses="w-1">
					<FileTree bind:tree level={0} {icons} bind:selected />
				</ScrollArea>
			</div>
		</Resizable.Pane>
		<Resizable.Handle />
		<Resizable.Pane defaultSize={80}>
			{#if $selected.type === 'markdown'}
				<ScrollArea class="h-[84vh]" orientation="vertical" scrollbarYClasses="w-1">
					<pre>
                        {$selected.value}
                    </pre>
				</ScrollArea>
			{/if}
		</Resizable.Pane>
	</Resizable.PaneGroup>
</div>
