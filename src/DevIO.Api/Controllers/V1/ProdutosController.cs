using AutoMapper;
using DevIO.Api.Extensions;
using DevIO.Api.ViewModels;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Repositories;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DevIO.Api.Controllers.V1
{
    [Authorize]
    [ApiVersion("1.0", Deprecated = true)]
    [Route("api/v{version:apiVersion}/produtos")]
    public class ProdutosController : MainController
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;

        public ProdutosController(INotificador notificador,
                                  IProdutoRepository produtoRepository,
                                  IProdutoService produtoService,
                                  IMapper mapper) : base(notificador)
        {
            _produtoRepository = produtoRepository;
            _produtoService = produtoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoViewModel>>> ObterTodos()
        {
            var produtosVM = _mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoRepository.ObterProdutosFornecedores());

            return CustomResponse(produtosVM);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId([FromRoute] Guid id)
        {
            var produtoVM = _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutoFornecedor(id));

            if (produtoVM == null) return NotFound("O produto não foi encontrado.");

            return CustomResponse(produtoVM);
        }

        [HttpPost]
        [ClaimsAuthorize("Produto", "Adicionar")]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar([FromBody] ProdutoViewModel produtoVM)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            string imagemNome = $"{Guid.NewGuid()}_{produtoVM.Imagem}";

            if (!await UploadImagem(produtoVM.ImagemUpload, imagemNome)) return CustomResponse(ModelState);

            var produto = _mapper.Map<Produto>(produtoVM);
            produto.Imagem = imagemNome;

            await _produtoService.Adicionar(produto);

            return CustomResponse(_mapper.Map<ProdutoViewModel>(produto));
        }

        [HttpPost("adicionar-produto-imagem")]
        [ClaimsAuthorize("Produto", "Adicionar")]
        public async Task<ActionResult<ProdutoViewModel>> AdicionarComImagemGrande([FromForm] ProdutoImagemViewModel produtoVM)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            string imagemPrefixo = $"{Guid.NewGuid()}_";

            if (!await UploadImagem(produtoVM.ImagemUpload, imagemPrefixo)) return CustomResponse(ModelState);

            var produto = _mapper.Map<Produto>(produtoVM);
            produto.Imagem = imagemPrefixo + produtoVM.ImagemUpload.FileName;

            await _produtoService.Adicionar(produto);

            return CustomResponse(_mapper.Map<ProdutoViewModel>(produto));
        }

        [HttpPut("{id:guid}")]
        [ClaimsAuthorize("Produto", "Atualizar")]
        public async Task<ActionResult<ProdutoViewModel>> Atualizar([FromRoute] Guid id, [FromBody] ProdutoViewModel produtoVM)
        {
            if (id != produtoVM.Id) return CustomErrorResponse("O ID informado não é o mesmo que foi passado na rota!");

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var produto = await _produtoRepository.Buscar(p => p.Id == id);

            if (produto == null) return NotFound("O produto não foi encontrado.");

            if (produtoVM.ImagemUpload != null)
            {
                if (!await DeletarImagem(produto.Imagem)) return CustomResponse(ModelState);

                string imagemNome = $"{Guid.NewGuid()}_{produtoVM.Imagem}";
                if (!await UploadImagem(produtoVM.ImagemUpload, imagemNome)) return CustomResponse(ModelState);

                produtoVM.Imagem = imagemNome;
            }

            var produtoAtualizar = _mapper.Map<Produto>(produtoVM);

            await _produtoService.Atualizar(produtoAtualizar);

            return CustomResponse(produtoVM);
        }

        [HttpPut("atualizar-produto-imagem/{id:guid}")]
        [ClaimsAuthorize("Produto", "Atualizar")]
        public async Task<ActionResult<ProdutoViewModel>> AtualizarComImagemGrande([FromRoute] Guid id, [FromForm] ProdutoImagemViewModel produtoVM)
        {
            if (id != produtoVM.Id) return CustomErrorResponse("O ID informado não é o mesmo que foi passado na rota!");

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var produto = await _produtoRepository.Buscar(p => p.Id == id);

            if (produto == null) return NotFound("O produto não foi encontrado.");

            if (produtoVM.ImagemUpload != null)
            {
                if (!await DeletarImagem(produto.Imagem)) return CustomResponse(ModelState);

                string imagemPrefixo = $"{Guid.NewGuid()}_";
                if (!await UploadImagem(produtoVM.ImagemUpload, imagemPrefixo)) return CustomResponse(ModelState);

                produtoVM.Imagem = imagemPrefixo + produtoVM.ImagemUpload.FileName;
            }

            var produtoAtualizar = _mapper.Map<Produto>(produtoVM);

            await _produtoService.Atualizar(produtoAtualizar);

            return CustomResponse(produtoVM);
        }

        [HttpDelete("{id:guid}")]
        [ClaimsAuthorize("Produto", "Excluir")]
        public async Task<IActionResult> Remover([FromRoute] Guid id)
        {
            var produto = await _produtoRepository.Buscar(p => p.Id == id);

            if (produto == null) return NotFound("O produto não foi encontrado.");

            if (!await DeletarImagem(produto.Imagem)) return CustomResponse(ModelState);

            await _produtoService.Remover(id);

            return CustomResponse();
        }

        private async Task<bool> UploadImagem(string arquivo, string imgNome)
        {
            if (string.IsNullOrWhiteSpace(arquivo))
            {
                ModelState.AddModelError(string.Empty, "Favor fornecer uma imagem para este produto!");
                return false;
            }

            byte[] imagemBytes = Convert.FromBase64String(arquivo);
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                ModelState.AddModelError(string.Empty, $"Já existe uma imagem com o nome {imgNome}!");
                return false;
            }

            await System.IO.File.WriteAllBytesAsync(filePath, imagemBytes);

            return true;
        }

        private async Task<bool> UploadImagem(IFormFile arquivo, string imgPrefixo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Favor forneça uma imagem para este produto!");
                return false;
            }

            string imgNome = imgPrefixo + arquivo.FileName;
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                ModelState.AddModelError(string.Empty, $"Já existe uma imagem com o nome {imgNome}!");
                return false;
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }

        private async Task<bool> DeletarImagem(string imgNome)
        {
            if (string.IsNullOrEmpty(imgNome)) return await Task.FromResult(true);

            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", imgNome);

            if (!System.IO.File.Exists(path))
            {
                ModelState.AddModelError(string.Empty, $"Não foi encontrado a imagem com o nome {imgNome}!");
                return await Task.FromResult(false);
            }

            System.IO.File.Delete(path);

            return await Task.FromResult(true);
        }
    }
}
