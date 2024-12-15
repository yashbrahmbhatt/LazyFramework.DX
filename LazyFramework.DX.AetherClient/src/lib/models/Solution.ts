export class Solution {
	Name: string;
	Root: string;
	Id: string;
	ExpressionLanguage: ExpressionLanguage;
	TargetFramework: string;
	Projects: UiPathProject[];
	StudioVersion: string;

	constructor(data: any) {
		this.Name = data.Name;
		this.Root = data.Root;
		this.Id = data.Id;
		this.ExpressionLanguage = data.ExpressionLanguage;
		this.TargetFramework = data.TargetFramework;
		this.Projects = data.Projects || {};
		this.StudioVersion = data.StudioVersion;
	}
}

// Supporting Types
export enum ExpressionLanguage {
	'VisualBasic' = 1,
	'CSharp' = 2
}

export interface UiPathProject {
	IsRoot: boolean;
	Path: string;
	Workflows: Editor[];
  Configuration: Config;

	Name: string;
	ProjectId: string;
	Description: string;
	Main: string;
	Dependencies: Record<string, string>;
	WebServices: object[];
	EntitiesStores: object[];
	SchemaVersion: string;
	StudioVersion: string;
	ProjectVersion: string;
	RuntimeOptions: RuntimeOptions;
	DesignOptions: DesignOptions;
	ExpressionLanguage: string;
	EntryPoints: EntryPoint[];
	IsTemplate: boolean;
	TemplateProjectData: Record<string, object>;
	PublishData: Record<string, object>;
	TargetFramework: string;
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

export interface ProcessOptions {
	ignoredFiles: string[];
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

export interface EntryPoint {
	filePath: string;
	uniqueId: string;
	input: object[];
	output: object[];
}

export interface DesignOptions {
	projectProfile: string;
	outputType: string;
	libraryOptions: LibraryOptions;
	processOptions: ProcessOptions;
	fileInfoCollection: FileInfo[];
	saveToCloud: boolean;
}

export interface Editor {
	Path: string;
	Class: string;
	Namespaces: Record<string, any>;
	References: Record<string, any>;
	Arguments: Argument[];
	Activities: Activity;
	Outline: any;
	Description: string;
	Expressions: Expression[];
}

export interface Variable {
	Name: string;
	Type: string;
	Description: string;
	DefaultValue: any;
}

export interface Argument {
	Name: string;
	Type: string;
	Direction: string;
	Description: string;
	DefaultValue: any;
}

export interface Activity {
	Name: string;
	Description: string;
	Type: Type;
  Properties: Argument[];

  Children: Activity[];
}
export interface Type {
  Assembly: string;
  Name: string;
}
export interface Expression {
	Path: string;
	Value: any;
	Type: Type;
  Parent: Activity;
}

export interface Config {
	Settings: Setting[];
	Assets: Asset[];
  Queues: Queues[];
	TextFiles: File[];
	ExcelFiles: File[];
}
export enum ConfigType{

}

export interface Queues {
  Name: string;
  Folder: string;
  Description: string;
}

export interface Asset {
	Name: string;
	Value: string;
	Folder: string;
	Description: string;
}

export interface File {
	Name: string;
	Path: string;
	Folder: string;
	Bucket: string;
	Description: string;
	Type: FileType;
}

export interface Setting {
	Name: string;
	Value: string;
	Description: string;
}

export type FileType = 'Unknown' | 'Text' | 'Excel' | 'Other';
