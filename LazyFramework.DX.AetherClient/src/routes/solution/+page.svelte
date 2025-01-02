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
	import XamlViewer from './XamlViewer.svelte';
	let { data }: { data: PageData } = $props();

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

	$inspect('page', $selected.type);
</script>

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
			{:else if $selected.type === 'workflow'}
				<XamlViewer xaml={$selected.value} />
			{:else if $selected.type === 'code'}
				<ScrollArea class="h-[84vh]" orientation="vertical" scrollbarYClasses="w-1">
					<pre>
						{$selected.value}
					</pre>
				</ScrollArea>
			{:else if $selected.type === 'file'}
				<ScrollArea class="h-[84vh]" orientation="vertical" scrollbarYClasses="w-1">
					<pre>
						{$selected.value}
					</pre>
				</ScrollArea>
			{:else if $selected.type === 'project'}
				<ScrollArea class="h-[84vh]" orientation="vertical" scrollbarYClasses="w-1">
					<pre>
						{$selected.value}
					</pre>
				</ScrollArea>
			{:else if $selected.type === 'solution'}
				<ScrollArea class="h-[84vh]" orientation="vertical" scrollbarYClasses="w-1">
					<pre>
						{$selected.value}
					</pre>
				</ScrollArea>
			{:else if $selected.type === 'entry'}
				<XamlViewer xaml={$selected.value} />
			{:else if $selected.type === 'json'}
				<ScrollArea class="h-[84vh]" orientation="vertical" scrollbarYClasses="w-1">
					<pre>
						{$selected.value}
					</pre>
				</ScrollArea>
			{:else if $selected.type === 'test'}
				<XamlViewer xaml={$selected.value} />
			{:else if $selected.type === 'folder'}
				<ScrollArea class="h-[84vh]" orientation="vertical" scrollbarYClasses="w-1">
					<pre>
						{$selected.value}
					</pre>
				</ScrollArea>
			{:else}
				Hello World
			{/if}
		</Resizable.Pane>
	</Resizable.PaneGroup>
</div>

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
