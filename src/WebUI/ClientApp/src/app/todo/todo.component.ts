import { Component, TemplateRef, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import {
  TodoListsClient, TodoItemsClient,
  TodoListDto, TodoItemDto, PriorityLevelDto,
  CreateTodoListCommand, UpdateTodoListCommand,
  CreateTodoItemCommand, UpdateTodoItemDetailCommand,
  TagDto, CreateTagsCommand, TagsClient,
  TodoItemTags
} from '../web-api-client';

@Component({
  selector: 'app-todo-component',
  templateUrl: './todo.component.html',
  styleUrls: ['./todo.component.scss']
})
export class TodoComponent implements OnInit {
  debug = false;
  deleting = false;
  deleteCountDown = 0;
  deleteCountDownInterval: any;
  lists: TodoListDto[];
  priorityLevels: PriorityLevelDto[];
  selectedList: TodoListDto;
  selectedItem: TodoItemDto;
  searchQuery: string = '';
  selectedTags: number[];
  tags: TagDto[];
  filteredItems: TodoItemDto[] = [];
  showTagInput: boolean = false;
  newTagName: string = '';
  newListEditor: any = {};
  listOptionsEditor: any = {};
  newListModalRef: BsModalRef;
  listOptionsModalRef: BsModalRef;
  deleteListModalRef: BsModalRef;
  itemDetailsModalRef: BsModalRef;
  itemDetailsFormGroup = this.fb.group({
    id: [null],
    listId: [null],
    priority: [''],
    note: [''],
    tags: [[]]
  });


  constructor(
    private listsClient: TodoListsClient,
    private itemsClient: TodoItemsClient,
    private modalService: BsModalService,
    private fb: FormBuilder,
    private tagsClient: TagsClient
  ) { }

  ngOnInit(): void {
    this.listsClient.get().subscribe(
      result => {
        this.lists = result.lists;
        this.priorityLevels = result.priorityLevels;
        this.tags = result.tags;
        console.log(this.lists);
        if (this.lists.length) {
          this.selectList(this.lists[1]);
        }
      },
      error => console.error(error)
    );
  }

  // Lists
  remainingItems(list: TodoListDto): number {
    return list.items.filter(t => !t.done).length;
  }

  selectList(list: TodoListDto): void {
    this.selectedList = list;
    this.selectedTags = [];
    this.searchQuery = '';
    this.filteredItems = [...list.items];
    this.filterItems();
  }

  showNewListModal(template: TemplateRef<any>): void {
    this.newListModalRef = this.modalService.show(template);
    setTimeout(() => document.getElementById('title').focus(), 250);
  }

  newListCancelled(): void {
    this.newListModalRef.hide();
    this.newListEditor = {};
  }

  addList(): void {
    const list = {
      id: 0,
      title: this.newListEditor.title,
      items: []
    } as TodoListDto;

    this.listsClient.create(list as CreateTodoListCommand).subscribe(
      result => {
        list.id = result;
        this.lists.push(list);
        this.selectedList = list;
        this.newListModalRef.hide();
        this.newListEditor = {};
      },
      error => {
        const errors = JSON.parse(error.response);

        if (errors && errors.Title) {
          this.newListEditor.error = errors.Title[0];
        }

        setTimeout(() => document.getElementById('title').focus(), 250);
      }
    );
  }

  showListOptionsModal(template: TemplateRef<any>) {
    this.listOptionsEditor = {
      id: this.selectedList.id,
      title: this.selectedList.title
    };

    this.listOptionsModalRef = this.modalService.show(template);
  }

  updateListOptions() {
    const list = this.listOptionsEditor as UpdateTodoListCommand;
    this.listsClient.update(this.selectedList.id, list).subscribe(
      () => {
        (this.selectedList.title = this.listOptionsEditor.title),
          this.listOptionsModalRef.hide();
        this.listOptionsEditor = {};
      },
      error => console.error(error)
    );
  }

  confirmDeleteList(template: TemplateRef<any>) {
    this.listOptionsModalRef.hide();
    this.deleteListModalRef = this.modalService.show(template);
  }

  deleteListConfirmed(): void {
    this.listsClient.delete(this.selectedList.id).subscribe(
      () => {
        this.deleteListModalRef.hide();
        this.lists = this.lists.filter(t => t.id !== this.selectedList.id);
        this.selectedList = this.lists.length ? this.lists[0] : null;
      },
      error => console.error(error)
    );
  }

  // Items
  showItemDetailsModal(template: TemplateRef<any>, item: TodoItemDto): void {
    this.selectedItem = item;
    
    // Reset form and patch with current item values
    this.itemDetailsFormGroup.reset();
    this.itemDetailsFormGroup.patchValue({
      id: item.id,
      listId: item.listId,
      priority: item.priority,
      note: item.note,
      tags: item.todoItemTagsList?.map(tag => tag.tagId) || []
    });

    this.itemDetailsModalRef = this.modalService.show(template);
    this.itemDetailsModalRef.onHidden.subscribe(() => {
        this.stopDeleteCountDown();
    });
  }

  updateItemDetails(): void {
    const item = new UpdateTodoItemDetailCommand(this.itemDetailsFormGroup.value);
    this.itemsClient.updateItemDetails(this.selectedItem.id, item).subscribe(
      () => {
        if (this.selectedItem.listId !== item.listId) {
          this.selectedList.items = this.selectedList.items.filter(
            i => i.id !== this.selectedItem.id
          );
          const listIndex = this.lists.findIndex(
            l => l.id === item.listId
          );
          this.selectedItem.listId = item.listId;
          this.lists[listIndex].items.push(this.selectedItem);
        }

        // Update the selected item's properties including tags
        this.selectedItem.priority = item.priority;
        this.selectedItem.note = item.note;
        this.selectedItem.todoItemTagsList = item.tags?.map(tagId => ({
          tagId: tagId,
          todoItemId: this.selectedItem.id
        } as TodoItemTags)) || [];

        // Update the item in both lists and filtered items
        const itemInList = this.selectedList.items.find(i => i.id === this.selectedItem.id);
        if (itemInList) {
          itemInList.todoItemTagsList = this.selectedItem.todoItemTagsList;
        }

        // Refresh the filtered items to reflect tag changes
        this.filterItems();

        this.itemDetailsModalRef.hide();
        this.itemDetailsFormGroup.reset();
      },
      error => console.error(error)
    );
  }

  addItem() {
    const item = {
      id: 0,
      listId: this.selectedList.id,
      priority: this.priorityLevels[0].value,
      title: '',
      done: false,
      backgroundColor: ''
    } as TodoItemDto;

    this.selectedList.items.push(item);
    this.filteredItems.push(item);
    const index = this.selectedList.items.length - 1;
    this.editItem(item, 'itemTitle' + index);
  }

  editItem(item: TodoItemDto, inputId: string): void {
    this.selectedItem = item;
    setTimeout(() => document.getElementById(inputId).focus(), 100);
  }

  updateItem(item: TodoItemDto, pressedEnter: boolean = false): void {
    const isNewItem = item.id === 0;

    if (!item.title.trim()) {
      this.deleteItem(item);
      return;
    }

    if (item.id === 0) {
      this.itemsClient
        .create({
          ...item, listId: this.selectedList.id
        } as CreateTodoItemCommand)
        .subscribe(
          result => {
            item.id = result;
          },
          error => console.error(error)
        );
    } else {
      this.itemsClient.update(item.id, item).subscribe(
        () => console.log('Update succeeded.'),
        error => console.error(error)
      );
    }

    this.selectedItem = null;

    if (isNewItem && pressedEnter) {
      setTimeout(() => this.addItem(), 250);
    }
  }

  deleteItem(item: TodoItemDto, countDown?: boolean) {
    if (countDown) {
      if (this.deleting) {
        this.stopDeleteCountDown();
        return;
      }
      this.deleteCountDown = 3;
      this.deleting = true;
      this.deleteCountDownInterval = setInterval(() => {
        if (this.deleting && --this.deleteCountDown <= 0) {
          this.deleteItem(item, false);
        }
      }, 1000);
      return;
    }
    this.deleting = false;
    if (this.itemDetailsModalRef) {
      this.itemDetailsModalRef.hide();
    }

    if (item.id === 0) {
      const itemIndex = this.selectedList.items.indexOf(this.selectedItem);
      this.selectedList.items.splice(itemIndex, 1);
      this.filteredItems = this.filteredItems.filter(t => t !== item);
    } else {
      this.itemsClient.delete(item.id).subscribe(
        () => {
          this.selectedList.items = this.selectedList.items.filter(t => t.id !== item.id);
          this.filteredItems = this.filteredItems.filter(t => t.id !== item.id);
        },
        error => console.error(error)
      );
    }
  }

  stopDeleteCountDown() {
    clearInterval(this.deleteCountDownInterval);
    this.deleteCountDown = 0;
    this.deleting = false;
  }

  filterItems(): void {
    if (!this.selectedList) {
       this.filteredItems = [];
       return;
    }

    this.filteredItems = this.selectedList.items.filter(item => {
      const matchesSearch = !this.searchQuery || 
        item.title.toLowerCase().includes(this.searchQuery.toLowerCase()) ||
        (item.note && item.note.toLowerCase().includes(this.searchQuery.toLowerCase()));
      
      const matchesTags = !this.selectedTags?.length || 
        (item.todoItemTagsList && item.todoItemTagsList.some(tag => this.selectedTags.includes(tag.tagId)));

      return matchesSearch && matchesTags;
    });
  }

  clearTagFilter(): void {
    this.selectedTags = [];
    this.filterItems();
  }

  showNewTagInput(): void {
    this.showTagInput = true;
  }

  saveNewTag(): void {
    if (this.newTagName.trim()) {
      const command = new CreateTagsCommand({ name: this.newTagName.trim() });
      this.tagsClient.createTag(command).subscribe(
        (id) => {
          const newTag = {
            id: id,
            name: this.newTagName.trim()
          } as TagDto;
          this.tags.push(newTag);
          this.newTagName = '';
          this.showTagInput = false;
        },
        error => console.error(error)
      );
    }
  }

  cancelNewTag(): void {
    this.newTagName = '';
    this.showTagInput = false;
  }
}
