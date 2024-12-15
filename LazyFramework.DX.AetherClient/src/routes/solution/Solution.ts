export interface SolutionTreeNode {
	id: string;
	type: 'project' | 'folder' | 'solution' | FileType;
	label: string;
	value: any;
	children?: SolutionTreeNode[];
	expanded?: boolean;
}
export interface LibraryOptions {
	includeOriginalXaml: boolean;
	privateWorkflows: string[];
}
export interface FileInfo {
	editingStatus: string;
	testCaseId?: string;
	testCaseType: string;
	executionTemplateType: string;
	fileName: string;
}
export interface ProcessOptions {
	ignoredFiles: string[];
}

export interface RuntimeOptions {
	autoDispose: boolean;
	netFrameworkLazyLoading: boolean;
	isPausable: boolean;
	isAttended: boolean;
	requiresUserInteraction: boolean;
	supportsPersistence: boolean;
	workflowSerialization: string;
	excludedLoggedData: string[];
	executionType: string;
	readyForPiP: boolean;
	startsInPiP: boolean;
	mustRestoreAllDependencies: boolean;
	pipType: string;
}

export interface DesignOptions {
	projectProfile: string;
	outputType: string;
	libraryOptions: LibraryOptions;
	processOptions: ProcessOptions;
	fileInfoCollection: FileInfo[];
	saveToCloud: boolean;
}
export interface EntryPoint {
	filePath: string;
	uniqueId: string;
	input: object[];
	output: object[];
}
export class ProjectJSON {
	dependencies: Record<string, string>;
	name: string;
	description: string;
	designOptions: DesignOptions;
	entitiesStores: Record<string, string>;
	entryPoints: EntryPoint[];
	expressionLanguage: string;
	isTemplate: boolean;
	main: string;
	projectId: string;
	projectVersion: string;
	publishData: any;
	runtimeOptions: RuntimeOptions;
	schemaVersion: string;
	studioVersion: string;
	targetFramework: string;
	templateProjectData: any;
	webServices: any[];
	path: string;

	constructor(rawJson: string, path: string) {
		//console.log('Initializing project JSON');
		const object = JSON.parse(rawJson);
		this.path = path;
		this.dependencies = object.dependencies as Record<string, string>; // Fixed typo
		this.name = object.name as string;
		this.description = object.description as string;
		this.designOptions = object.designOptions as DesignOptions;
		this.entitiesStores = object.entitiesStores as Record<string, string>;
		this.entryPoints = object.entryPoints as EntryPoint[];
		this.expressionLanguage = object.expressionLanguage as string;
		this.isTemplate = object.isTemplate as boolean;
		this.main = object.main as string;
		this.projectId = object.projectId as string;
		this.projectVersion = object.projectVersion as string;
		this.publishData = object.publishData as any;
		this.runtimeOptions = object.runtimeOptions as RuntimeOptions;
		this.schemaVersion = object.schemaVersion as string;
		this.studioVersion = object.studioVersion as string;
		this.targetFramework = object.targetFramework as string;
		this.templateProjectData = object.templateProjectData as any;
		this.webServices = object.webServices as any[];
		//console.log('Project JSON initialized', this);
	}
}

export class Solution {
	projects: Project[];
	sharedFiles: Record<string, string>; // List of files shared across all entry points
	allFiles: Record<string, string>; // List of all files in the solution

	constructor(files: Record<string, string>) {
		this.allFiles = files;
		//console.log('Initializing solution');
		let projectJsons = Object.keys(files)
			.filter((file) => file.endsWith('project.json'))
			.map((file) => new ProjectJSON(files[file], file));
		//console.log('Project JSONs initialized', projectJsons);
		this.projects = projectJsons.map((json) => new Project(json, files));
		//console.log('Projects initialized', this.projects);
		this.sharedFiles = this.getSharedFilesContent(files);
		//console.log('Solution initialized', this);
	}

	/**
	 * Finds files that are shared across all entry points in all projects
	 * and returns them as a Record<string, string> with file paths as keys and contents as values.
	 * @param {Record<string, string>} files - The file map containing all file paths and contents.
	 * @returns {Record<string, string>} A record of shared file paths and their corresponding contents.
	 */
	private getSharedFilesContent(files: Record<string, string>): Record<string, string> {
		const sharedFilesPaths = this.getSharedFiles();
		const sharedFiles: Record<string, string> = {};

		sharedFilesPaths.forEach((filePath) => {
			if (files[filePath]) {
				sharedFiles[filePath] = files[filePath];
			} else {
				console.warn(`Shared file not found: ${filePath}`);
			}
		});

		return sharedFiles;
	}

	/**
	 * Finds files that are shared across all entry points in all projects.
	 * @returns {string[]} Array of shared absolute file paths.
	 */
	private getSharedFiles(): string[] {
		const allFileSets: Set<string>[] = this.projects.map((project) => {
			const projectFiles = Object.values(project.entryPointFiles)
				.flat()
				.map((entry) => entry.entryPoint.filePath);
			return new Set(projectFiles);
		});

		if (allFileSets.length === 0) return [];

		const sharedFilesSet = allFileSets.reduce((shared, currentSet) => {
			return new Set([...shared].filter((file) => currentSet.has(file)));
		}, allFileSets[0]);

		return Array.from(sharedFilesSet);
	}
	public convertSolutionToTree(solution: Solution = this): SolutionTreeNode {
		const rootNode: SolutionTreeNode = {
			id: 'solution', // Unique ID for the solution node
			type: 'solution', // Type is 'solution' for the root node
			label: 'Solution', // Label for the root node (could be the solution name)
			value: solution, // Store the entire solution object as the value
			children: [], // Will contain folder nodes and file nodes
			expanded: true // Set expanded to true by default
		};

		// Iterate over all the files and organize them into a tree structure
		Object.keys(solution.allFiles).forEach((filePath) => {
			// If the file is a project.json, treat it specially
			this.createFileNode(filePath, solution.allFiles[filePath], rootNode); // Project file
		});
		while (
			rootNode.children &&
			rootNode.children.length === 1 &&
			rootNode.children[0].type === 'folder'
		) {
			rootNode.children = rootNode.children[0].children;
		}
		rootNode.children?.sort((a, b) => {
			if (a.type === 'folder' && b.type !== 'folder') {
				return -1;
			} else if (a.type !== 'folder' && b.type === 'folder') {
				return 1;
			} else {
				return a.label.localeCompare(b.label);
			}
		});
		return rootNode;
	}

	/**
	 * Creates a file node and places it in the correct folder structure.
	 * @param {string} filePath - The path of the file.
	 * @param {string} fileContent - The content of the file.
	 * @param {SolutionTreeNode} parentNode - The parent node to attach the file node to.
	 * @param {boolean} isProjectFile - Indicates whether the file is a project.json.
	 */
	public createFileNode(filePath: string, fileContent: string, parentNode: SolutionTreeNode) {
		const pathParts = filePath.split(/[/\\]/); // Split the file path by slashes or backslashes
		let currentNode = parentNode; // Start at the root node

		// Traverse the path parts and create folder nodes if needed
		for (let i = 0; i < pathParts.length - 1; i++) {
			const folderName = pathParts[i];
			let folderNode = this.findFolderNode(currentNode, folderName);

			if (!folderNode) {
				folderNode = this.createFolderNode(folderName); // Create new folder if not found
				currentNode.children?.push(folderNode); // Add folder node as a child
				currentNode.children?.sort((a, b) => {
					if (a.type === 'folder' && b.type !== 'folder') {
						return -1;
					} else if (a.type !== 'folder' && b.type === 'folder') {
						return 1;
					} else {
						return a.label.localeCompare(b.label);
					}
				});
			}

			currentNode = folderNode; // Move down the tree
		}

		// Create a file node
		const fileNode: SolutionTreeNode = {
			id: `file-${filePath}`, // Unique ID for the file node
			type: this.getFileType(filePath), // Type is 'project' for project.json files, 'file' otherwise
			label: pathParts[pathParts.length - 1], // Label is the file name
			value:
				this.getFileType(filePath) === FileType.project
					? new ProjectJSON(fileContent, filePath)
					: fileContent, // Set value as ProjectJSON for project.json files
			children: [], // Files do not have children by default
			expanded: false // Set expanded to false by default
		};

		// Add the file node to the current folder node
		currentNode.children?.push(fileNode);
	}

	public getFileType(filePath: string): FileType {
		if (filePath.endsWith('.xaml')) {
			if (
				this.projects.some((project) => {
					return project.entryPointFiles.some(
						(entry) =>
							getDirectory(project.json.path) + '\\' + entry.entryPoint.filePath === filePath
					);
				})
			) {
				return FileType.entry;
			} else if (
				this.projects.some((project) => {
					return project.json.designOptions.fileInfoCollection.some(
						(fileInfo) =>
							getDirectory(project.json.path) + '\\' + fileInfo.fileName === filePath &&
							fileInfo.testCaseId !== undefined
					);
				})
			) {
				return FileType.test;
			} else {
				return FileType.workflow;
			}
		} else if (filePath.endsWith('.xlsx') || filePath.endsWith('.xls')) {
			return FileType.excel;
		} else if (filePath.endsWith('.md')) {
			return FileType.markdown;
		} else if (filePath.endsWith('project.json')) {
			return FileType.project;
		} else if (filePath.endsWith('.json')) {
			return FileType.json;
		} else if (filePath.endsWith('.cs') || filePath.endsWith('.vb')) {
			return FileType.code;
		} else {
			return FileType.other;
		}
	}

	/**
	 * Finds a folder node by name under the given parent node.
	 * @param {SolutionTreeNode} parentNode - The parent node to search under.
	 * @param {string} folderName - The name of the folder to find.
	 * @returns {SolutionTreeNode | undefined} The folder node if found, otherwise undefined.
	 */
	public findFolderNode(
		parentNode: SolutionTreeNode,
		folderName: string
	): SolutionTreeNode | undefined {
		return parentNode.children?.find((node) => node.type === 'folder' && node.label === folderName);
	}

	/**
	 * Creates a new folder node.
	 * @param {string} folderName - The name of the folder.
	 * @returns {SolutionTreeNode} The newly created folder node.
	 */
	public createFolderNode(folderName: string): SolutionTreeNode {
		return {
			id: `folder-${folderName}`, // Unique ID for the folder node
			type: 'folder', // Type is 'folder' for folder nodes
			label: folderName, // Label is the folder name
			value: folderName, // Value can be the folder name or other folder-related data
			children: [], // Folders can have children (other files or subfolders)
			expanded: false // Set expanded to true by default for folders
		};
	}
}
export interface ProjectEntryPoint {
	entryPoint: EntryPoint;
	invokedWorkflows: string[];
}

export class Project {
	json: ProjectJSON;
	entryPointFiles: ProjectEntryPoint[];

	constructor(proj: ProjectJSON, files: Record<string, string>) {
		//console.log('Initializing project');
		this.json = proj;
		const basePath = 'C:\\Users\\eyash\\OneDrive\\Documents\\UiPath\\RoboticEnterpriseFramework4';
		this.entryPointFiles = proj.entryPoints.map((entry) => {
			var entryFilePath = resolvePath(basePath, entry.filePath);
			return {
				entryPoint: entry,
				invokedWorkflows: Array.from(getAllInvokedWorkflows(files, entryFilePath))
			} as ProjectEntryPoint;
		});
		//console.log('Project initialized', this);
	}
}

/**
 * Resolves a relative file path to an absolute path.
 * @param {string} basePath - The base directory.
 * @param {string} relativePath - The relative file path.
 * @returns {string} The resolved absolute file path.
 */
export function resolvePath(basePath: string, relativePath: string): string {
	const baseParts = basePath.split(/[/\\]/); // Split by both slashes
	const relativeParts = relativePath.split(/[/\\]/);
	const resultParts = [...baseParts];

	for (const part of relativeParts) {
		if (part === '..') {
			resultParts.pop();
		} else if (part !== '.' && part !== '') {
			resultParts.push(part);
		}
	}

	return resultParts.join('\\');
}

export enum FileType {
	other = 'file',
	workflow = 'workflow',
	excel = 'excel',
	markdown = 'markdown',
	project = 'project',
	json = 'json',
	entry = 'entry',
	test = 'test',
	code = 'code'
}

/**
 * Recursively finds all invoked workflows and their absolute paths.
 * @param {Record<string, string>} fileMap - A record where keys are absolute file paths and values are XML strings.
 * @param {string} entryFilePath - The entry point absolute file path to start the recursion.
 * @returns {Set<string>} A set of all absolute file paths used in the workflow.
 */
export function getAllInvokedWorkflows(
	fileMap: Record<string, string>,
	entryFilePath: string
): Set<string> {
	const visitedPaths = new Set<string>();

	function recurse(currentFilePath: string) {
		// Skip already visited files to prevent infinite loops
		if (visitedPaths.has(currentFilePath)) {
			return;
		}
		visitedPaths.add(currentFilePath);

		const xmlContent = fileMap[currentFilePath];
		if (!xmlContent) {
			console.warn(`File not found: ${currentFilePath}`);
			return;
		}

		let xmlDoc: Document;
		try {
			// Parse the XML content
			xmlDoc = new DOMParser().parseFromString(xmlContent, 'application/xml');
		} catch (error) {
			console.error(`Failed to parse XML in file: ${currentFilePath}`, error);
			return;
		}

		// Check for parsing errors
		if (xmlDoc.querySelector('parsererror')) {
			console.error(`Invalid XML content in file: ${currentFilePath}`);
			return;
		}

		// Find all <ui:InvokeWorkflowFile> elements
		const invokeElements = xmlDoc.getElementsByTagName('ui:InvokeWorkflowFile');
		//console.log('Invoked Elements', invokeElements);
		for (let i = 0; i < invokeElements.length; i++) {
			const element = invokeElements[i];
			const workflowFileName = element.getAttribute('WorkflowFileName');

			if (workflowFileName) {
				// Resolve relative path to absolute path
				const absolutePath = resolvePath(getDirectory(currentFilePath), workflowFileName);

				if (fileMap[absolutePath]) {
					// Recursively process the invoked workflow file
					recurse(absolutePath);
				} else {
					console.warn(`Referenced file not found: ${absolutePath}`);
				}
			}
		}
	}

	recurse(entryFilePath); // Start recursion from the entry file
	return visitedPaths; // Return all the absolute paths that were visited
}

export function getDirectory(filePath: string): string {
	const lastSlashIndex = filePath.lastIndexOf('/');
	const lastBackslashIndex = filePath.lastIndexOf('\\');

	// Find the last slash (forward or backward depending on the platform)
	const lastIndex = Math.max(lastSlashIndex, lastBackslashIndex);

	// If a slash is found, return the substring before it as the directory
	if (lastIndex !== -1) {
		return filePath.substring(0, lastIndex);
	}

	// If no slashes are found, it's either a root or a file with no directory
	return ''; // Return empty string if no directory is found
}
