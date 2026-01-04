import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';

type DsaUsageRow = {
  structure: string;
  backendFiles: string;
  whereUsed: string;
  purpose: string;
};

@Component({
  selector: 'app-dsa-usage',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dsa-usage.component.html',
  styleUrls: ['./dsa-usage.component.css']
})
export class DsaUsageComponent {
  rows: DsaUsageRow[] = [
    {
      structure: 'Queue (FIFO) — DsaQueue<T>',
      backendFiles: 'DataStructures/DsaQueue.cs',
      whereUsed:
        'CoreIntelligence/Graph.cs (BFS course order), Services/ProgressGraph/ProgressGraphService.cs (BFS levels), DataStructures/Graph/DirectedGraph.cs (Kahn topological order)',
      purpose:
        'Supports breadth-first traversals (level/order computation) without using Queue<T>.'
    },
    {
      structure: 'Singly Linked List — LinkedList<T>',
      backendFiles: 'DataStructures/LinkedList.cs',
      whereUsed:
        'DataStructures/HashTable.cs (chaining buckets), DataStructures/Graph/* (node storage + adjacency lists + DFS stack helper)',
      purpose:
        'Provides basic dynamic storage for graph adjacency and hash buckets.'
    },
    {
      structure: 'Hash Table — HashTable<TKey, TValue>',
      backendFiles: 'DataStructures/HashTable.cs',
      whereUsed: 'DataStructures/Graph/DirectedGraph.cs (_byId node lookup)',
      purpose:
        'Maps node IDs to nodes without using Dictionary; enables efficient graph algorithms.'
    },
    {
      structure: 'Directed Graph (adjacency list) — DirectedGraph',
      backendFiles: 'DataStructures/Graph/DirectedGraph.cs, DataStructures/Graph/GraphNode.cs',
      whereUsed: 'Services/ProgressGraph/ProgressGraphService.cs (build progress graph)',
      purpose:
        'Builds an acyclic graph of user → skills → courses + metric nodes; supports cycle-safe edge creation and traversal-based calculations.'
    },
    {
      structure: 'Merge Sort — MergeSort.Sort',
      backendFiles: 'DataStructures/Sorting/MergeSort.cs',
      whereUsed: 'CoreIntelligence/LearningPathBuilder.cs (deterministic course ordering)',
      purpose:
        'Stable sorting of courses by level/sequence to generate consistent learning paths.'
    },
    {
      structure: 'Binary “Question Tree” (complete tree via indices) — BinaryQuestionTree',
      backendFiles: 'DataStructures/Trees/BinaryQuestionTree.cs',
      whereUsed: 'Services/SkillAssessment/SkillAssessmentService.cs (assessment traversal)',
      purpose:
        'Implements adaptive question navigation using TreeIndex rules (left=2k+1, right=2k+2) for skill assessments.'
    },
    {
      structure: 'Binary Tree Node — QuestionNode',
      backendFiles: 'DataStructures/Trees/QuestionNode.cs',
      whereUsed: 'Currently not referenced by services/controllers',
      purpose:
        'Classic binary node structure present for DSA completeness; assessment currently uses index-based tree navigation instead.'
    }
  ];
}
